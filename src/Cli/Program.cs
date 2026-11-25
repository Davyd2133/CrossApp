using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using CrossApp.Lab09.Configuration;
using CrossApp.Lab09.Domain;
using CrossApp.Lab09.Storage;
using Microsoft.Extensions.Configuration;

return Run(args);

static int Run(string[] args)
{
    try
    {
        var configurationArgs = args
            .Where(arg => arg.StartsWith("--", StringComparison.Ordinal) && arg.Contains('='))
            .ToArray();

        args = args
            .Where(arg => !arg.StartsWith("--", StringComparison.Ordinal) || !arg.Contains('='))
            .ToArray();
        var baseDirectory = AppContext.BaseDirectory;
        var profile = Environment.GetEnvironmentVariable("CROSSAPP_ENVIRONMENT");

        if (string.IsNullOrWhiteSpace(profile))
            profile = "Production";

        var config = new ConfigurationBuilder()
            .SetBasePath(baseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{profile}.json", optional: true)
            .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
            .AddEnvironmentVariables(prefix: "CROSSAPP_")
            .AddCommandLine(configurationArgs)
            .Build();

        var options = config.Get<AppOptions>() ?? new AppOptions();

        var dataPathOverride =
            Environment.GetEnvironmentVariable("CROSSAPP_DATA_PATH");

        if (!string.IsNullOrWhiteSpace(dataPathOverride))
            options.Storage.DataPath = dataPathOverride;

        var catalogPath = options.Storage.ResolveFullPath(baseDirectory);
        var store = new FileCatalogStore(catalogPath);
        var currency = options.Orders.Currency;

        Console.WriteLine($"Файл даних: {catalogPath}");

        if (string.Equals(options.Logging.MinimumLevel, "Debug",
                StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine($"[Debug] Профіль: {profile}");
        }

        var apiKey = config["Integration:ApiKey"];
        Console.WriteLine(string.IsNullOrWhiteSpace(apiKey)
            ? "ApiKey: не заданий"
            : $"ApiKey: присутній, довжина {apiKey.Length}");

        if (args.Length == 0)
        {
            PrintUsage();
            return 0;
        }

        switch (args[0].ToLowerInvariant())
        {
            case "list" when args.Length == 1:
                PrintOrders(store.GetAll(), currency);
                return 0;

            case "add" when args.Length == 4:
                return AddOrder(store, args, currency);

            case "export" when args.Length is 1 or 2:
            {
                var destination = args.Length == 2 ? args[1] : catalogPath;
                var count = store.ExportJson(destination);
                Console.WriteLine($"Експортовано замовлень: {count}");
                Console.WriteLine($"Файл експорту: {Path.GetFullPath(destination)}");
                return 0;
            }

            case "import" when args.Length == 2:
            {
                var count = store.ImportJson(args[1]);
                Console.WriteLine($"Імпортовано замовлень: {count}");
                return 0;
            }

            default:
                PrintUsage();
                return 1;
        }
    }
    catch (JsonException ex)
    {
        Console.Error.WriteLine($"Некоректний JSON або дані замовлення: {ex.Message}");
        return 1;
    }
    catch (Exception ex) when (
        ex is IOException or UnauthorizedAccessException or ArgumentException or FormatException)
    {
        Console.Error.WriteLine($"Помилка: {ex.Message}");
        return 1;
    }
}

static int AddOrder(FileCatalogStore store, string[] args, string currency)
{
    var productName = args[1].Trim();

    if (string.IsNullOrWhiteSpace(productName) ||
        !int.TryParse(args[2], out var quantity) || quantity <= 0 ||
        !decimal.TryParse(args[3], NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture, out var price))
    {
        Console.Error.WriteLine("Вкажи назву, цілу кількість від 1 і ціну. Приклад ціни: 450.50.");
        return 1;
    }

    var order = new OrderDto(Guid.NewGuid(), new List<OrderLineDto>
    {
        new(productName, quantity, price)
    });

    store.Add(order);
    Console.WriteLine($"Додано замовлення {order.Id}. Сума: {quantity * price:F2} {currency}");
    return 0;
}

static void PrintOrders(IReadOnlyList<OrderDto> orders, string currency)
{
    if (orders.Count == 0)
    {
        Console.WriteLine("У каталозі поки немає замовлень.");
        return;
    }

    foreach (var order in orders)
    {
        Console.WriteLine($"Замовлення {order.Id}");

        foreach (var line in order.Lines)
        {
            Console.WriteLine($"  {line.ProductName} × {line.Quantity}, ціна {line.Price:F2} {currency}");
        }

        var total = order.Lines.Sum(line => line.Quantity * line.Price);
        Console.WriteLine($"  Разом: {total:F2} {currency}");
    }
}

static void PrintUsage()
{
    Console.WriteLine("Команди:");
    Console.WriteLine("  list");
    Console.WriteLine("  add <назва товару> <кількість> <ціна>");
    Console.WriteLine("  export [файл.json]");
    Console.WriteLine("  import <файл.json>");
}