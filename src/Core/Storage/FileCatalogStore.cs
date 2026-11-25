using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using CrossApp.Lab09.Domain;

namespace CrossApp.Lab09.Storage;

public sealed class FileCatalogStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly string _path;

    public FileCatalogStore(string path)
    {
        _path = Path.GetFullPath(path);
    }

    public IReadOnlyList<OrderDto> GetAll()
    {
        return File.Exists(_path)
            ? ReadOrders(_path)
            : new List<OrderDto>();
    }

    public int ExportJson(string destinationPath)
    {
        var orders = GetAll();
        WriteOrders(destinationPath, orders);
        return orders.Count;
    }

    public int ImportJson(string sourcePath)
    {
        var orders = ReadOrders(sourcePath);
        WriteOrders(_path, orders);
        return orders.Count;
    }

    public void Add(OrderDto order)
    {
        ValidateOrder(order);

        var orders = GetAll().ToList();
        if (orders.Any(existing => existing.Id == order.Id))
            throw new ArgumentException($"Замовлення {order.Id} уже існує.");

        orders.Add(order);
        WriteOrders(_path, orders);
    }

    private static List<OrderDto> ReadOrders(string path)
    {
        using var stream = File.OpenRead(path);

        var orders = JsonSerializer.Deserialize<List<OrderDto>>(stream, JsonOptions)
            ?? throw new JsonException("Очікується JSON-масив замовлень.");

        var orderIds = new HashSet<Guid>();

        foreach (var order in orders)
        {
            ValidateOrder(order);

            if (!orderIds.Add(order.Id))
                throw new JsonException($"Повторюється ідентифікатор {order.Id}.");
        }

        return orders;
    }

    private static void ValidateOrder(OrderDto? order)
    {
        if (order is null || order.Id == Guid.Empty)
            throw new JsonException("Кожне замовлення повинно мати непорожній id.");

        if (order.Lines is null || order.Lines.Count == 0)
            throw new JsonException($"Замовлення {order.Id} повинно мати позиції.");

        foreach (var line in order.Lines)
        {
            if (line is null || string.IsNullOrWhiteSpace(line.ProductName))
                throw new JsonException($"У замовленні {order.Id} є позиція без назви.");

            if (line.Quantity <= 0)
                throw new JsonException($"Кількість для «{line.ProductName}» має бути більшою за 0.");

            if (line.Price < 0)
                throw new JsonException($"Ціна для «{line.ProductName}» має бути невід’ємною.");
        }
    }

    private static void WriteOrders(string path, IReadOnlyList<OrderDto> orders)
    {
        var fullPath = Path.GetFullPath(path);
        var directory = Path.GetDirectoryName(fullPath)!;
        Directory.CreateDirectory(directory);

        var temporaryPath = Path.Combine(
            directory, $"{Path.GetFileName(fullPath)}.{Guid.NewGuid():N}.tmp");

        try
        {
            using (var stream = new FileStream(temporaryPath, FileMode.CreateNew))
            {
                JsonSerializer.Serialize(stream, orders, JsonOptions);
            }

            File.Move(temporaryPath, fullPath, overwrite: true);
        }
        finally
        {
            File.Delete(temporaryPath);
        }
    }
}