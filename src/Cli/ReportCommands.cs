using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Order = Core.Domain.Order;
using CrossApp.Core.Reports;

namespace CrossApp.Cli;

public static class ReportCommands
{
    private static readonly CultureInfo UkrainianCulture =
        CultureInfo.GetCultureInfo("uk-UA");

    public static bool TryExecute(
        string[] args,
        IEnumerable<Order> orders)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(orders);

        // Шукаємо саме report-команду.
        // Прапорець --file не сприймається як назва звіту.
        int commandIndex = Array.FindIndex(
            args,
            argument => argument.StartsWith(
                "report-",
                StringComparison.Ordinal));

        if (commandIndex < 0)
        {
            return false;
        }

        string command = args[commandIndex];

        try
        {
            switch (command)
            {
                case "report-top":
                {
                    int count = ReadCount(
                        args,
                        commandIndex,
                        defaultValue: 5);

                    PrintCustomers(
                        ReportService.TopCustomers(orders, count));

                    break;
                }

                case "report-above":
                {
                    decimal threshold = ReadThreshold(
                        args,
                        commandIndex,
                        defaultValue: 1000m);

                    Console.WriteLine(
                        $"Замовлення дорожче {Money(threshold)} грн");
                    Console.WriteLine();

                    PrintOrders(
                        ReportService.OrdersAbove(
                            orders,
                            threshold));

                    break;
                }

                case "report-groups":
                {
                    PrintGroups(
                        ReportService.ByStatus(orders));

                    break;
                }

                case "report-sum":
                {
                    PrintTotals(
                        ReportService.Totals(orders));

                    break;
                }

                case "report-deferred":
                {
                    PrintDeferredDemo(orders);

                    break;
                }

                default:
                {
                    PrintHelp();
                    break;
                }
            }
        }
        catch (ArgumentException exception)
        {
            Console.WriteLine($"Помилка: {exception.Message}");
        }

        return true;
    }

    private static void PrintCustomers(
        IReadOnlyList<CustomerTotalDto> clients)
    {
        Console.WriteLine("ТОП КЛІЄНТІВ ЗА СУМОЮ ЗАМОВЛЕНЬ");
        Console.WriteLine();

        Console.WriteLine(
            $"{"Клієнт",-30} " +
            $"{"Замовлень",10} " +
            $"{"Сума, грн",16}");

        Console.WriteLine(new string('-', 58));

        foreach (var client in clients)
        {
            Console.WriteLine(
                $"{client.CustomerName,-30} " +
                $"{client.OrderCount,10} " +
                $"{Money(client.TotalAmount),16}");
        }

        if (clients.Count == 0)
        {
            Console.WriteLine("Дані відсутні.");
        }
    }

    private static void PrintOrders(
        IReadOnlyList<OrderReportDto> orders)
    {
        Console.WriteLine(
            $"{"ID",-14} " +
            $"{"Клієнт",-26} " +
            $"{"Статус",-14} " +
            $"{"Сума, грн",14}");

        Console.WriteLine(new string('-', 72));

        foreach (var order in orders)
        {
            Console.WriteLine(
                $"{order.Id,-14} " +
                $"{order.CustomerName,-26} " +
                $"{order.Status,-14} " +
                $"{Money(order.Total),14}");
        }

        if (orders.Count == 0)
        {
            Console.WriteLine(
                "Замовлень, які відповідають умові, немає.");
        }
    }

    private static void PrintGroups(
        IReadOnlyList<StatusSummaryDto> groups)
    {
        Console.WriteLine("ЗАМОВЛЕННЯ ЗА СТАТУСАМИ");
        Console.WriteLine();

        Console.WriteLine(
            $"{"Статус",-20} " +
            $"{"Замовлень",10} " +
            $"{"Сума, грн",16}");

        Console.WriteLine(new string('-', 48));

        foreach (var group in groups)
        {
            Console.WriteLine(
                $"{group.Status,-20} " +
                $"{group.OrderCount,10} " +
                $"{Money(group.TotalAmount),16}");
        }

        if (groups.Count == 0)
        {
            Console.WriteLine("Дані відсутні.");
        }
    }

    private static void PrintTotals(
        OrderTotalsDto totals)
    {
        Console.WriteLine("ЗАГАЛЬНІ ПІДСУМКИ");
        Console.WriteLine();

        Console.WriteLine(
            $"Кількість замовлень: {totals.OrderCount}");

        Console.WriteLine(
            $"Загальна сума: {Money(totals.TotalAmount)} грн");

        Console.WriteLine(
            $"Середній чек: {Money(totals.AverageAmount)} грн");

        Console.WriteLine(
            $"Максимальне замовлення: " +
            $"{Money(totals.MaxAmount)} грн");
    }

    private static void PrintDeferredDemo(
        IEnumerable<Order> orders)
    {
        // Окрема локальна колекція.
        var source = orders.ToList();

        // Поріг 0: у демонстрації враховуються
        // замовлення з додатною сумою.
        const decimal threshold = 0m;

        var query = ReportService.DeferredOrdersAbove(
            source,
            threshold);

        Console.WriteLine("ВІДКЛАДЕНЕ ВИКОНАННЯ LINQ");
        Console.WriteLine();

        int before = query.Count();

        Console.WriteLine(
            $"Кількість до зміни джерела: {before}");

        // Знаходимо наявне замовлення з додатною сумою.
        // Воно потрібне лише для зміни локального списку.
        var matchingOrder = source.Find(
            order => order.Total > threshold);

        if (matchingOrder is null)
        {
            Console.WriteLine(
                "Для демонстрації потрібне хоча б одне " +
                "замовлення з додатною сумою.");

            return;
        }

        // Повторно додаємо посилання тільки до локального списку.
        // Сховище та справжні замовлення не змінюються.
        source.Add(matchingOrder);

        int after = query.Count();

        Console.WriteLine(
            $"Кількість після додавання: {after}");

        // Запит виконується, а його результат зберігається.
        var snapshot = query.ToList();

        Console.WriteLine(
            $"Кількість у збереженому результаті: " +
            $"{snapshot.Count}");

        source.Clear();

        Console.WriteLine(
            $"Кількість у запиті після очищення джерела: " +
            $"{query.Count()}");

        Console.WriteLine(
            $"Кількість у збереженому результаті " +
            $"після очищення джерела: {snapshot.Count}");

        Console.WriteLine();
        Console.WriteLine(
            "Count() повторно виконує запит і враховує " +
            "поточний вміст джерела.");

        Console.WriteLine(
            "ToList() зберігає результат в окремому списку.");
    }

    private static int ReadCount(
        string[] args,
        int commandIndex,
        int defaultValue)
    {
        string? value = ReadArgumentAfterCommand(
            args,
            commandIndex);

        if (value is null)
        {
            return defaultValue;
        }

        if (!int.TryParse(
                value,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out int count)
            || count < 0)
        {
            throw new ArgumentException(
                "Кількість має бути цілим невід’ємним числом.");
        }

        return count;
    }

    private static decimal ReadThreshold(
        string[] args,
        int commandIndex,
        decimal defaultValue)
    {
        string? value = ReadArgumentAfterCommand(
            args,
            commandIndex);

        if (value is null)
        {
            return defaultValue;
        }

        // Приймаємо і 1000.50, і 1000,50.
        string normalized = value.Replace(',', '.');

        if (!decimal.TryParse(
                normalized,
                NumberStyles.AllowLeadingSign |
                NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out decimal threshold)
            || threshold < 0)
        {
            throw new ArgumentException(
                "Поріг має бути невід’ємним числом.");
        }

        return threshold;
    }

    private static string? ReadArgumentAfterCommand(
        string[] args,
        int commandIndex)
    {
        int argumentIndex = commandIndex + 1;

        if (argumentIndex >= args.Length)
        {
            return null;
        }

        string argument = args[argumentIndex];

        if (argument.StartsWith(
                "--",
                StringComparison.Ordinal))
        {
            return null;
        }

        return argument;
    }

    private static string Money(decimal amount)
    {
        return amount.ToString("F2", UkrainianCulture);
    }

    private static void PrintHelp()
    {
        Console.WriteLine("Команди звітів:");
        Console.WriteLine();

        Console.WriteLine(
            "report-top [N]       — топ клієнтів, типово 5");

        Console.WriteLine(
            "report-above [сума]   — замовлення дорожче суми");

        Console.WriteLine(
            "report-groups        — групування за статусом");

        Console.WriteLine(
            "report-sum           — загальні підсумки");

        Console.WriteLine(
            "report-deferred      — відкладене виконання");
    }
}