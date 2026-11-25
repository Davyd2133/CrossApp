using System;
using System.Collections.Generic;
using System.Linq;
using CrossApp.Core.Reports;
using Order = Core.Domain.Order;

namespace CrossApp.Cli;

public static class AdditionalReportCommands
{
    public static bool TryExecute(
        string[] args,
        IEnumerable<Order> orders)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(orders);

        int index = Array.FindIndex(
            args,
            argument => argument.StartsWith(
                "report-",
                StringComparison.Ordinal));

        if (index < 0)
        {
            return false;
        }

        string command = args[index];

        switch (command)
        {
            case "report-join":
                PrintJoinedReport(orders);
                return true;

            case "report-top-ext":
                PrintTopReports(orders);
                return true;

            case "report-query":
                PrintQueryReport(orders);
                return true;

            case "report-benchmark":
                PrintBenchmark();
                return true;

            default:
                return false;
        }
    }

    private static void PrintJoinedReport(
        IEnumerable<Order> orders)
    {
        // Незалежна колекція клієнтів.
        // У Наталії немає замовлень у SampleData.
        var customers = new[]
        {
            new ReportCustomer("Давид"),
            new ReportCustomer("Олена"),
            new ReportCustomer("Андрій"),
            new ReportCustomer("Марія"),
            new ReportCustomer("Іван"),
            new ReportCustomer("Софія"),
            new ReportCustomer("Наталія")
        };

        var rows = AdditionalReportService.CustomersWithOrders(
            customers,
            orders);

        Console.WriteLine("КЛІЄНТИ ТА ЇХНІ ЗАМОВЛЕННЯ");
        Console.WriteLine();

        Console.WriteLine(
            $"{"Клієнт",-20} " +
            $"{"Кількість",10} " +
            $"{"Сума, грн",14} " +
            $"{"Середній чек",14}");

        Console.WriteLine(new string('-', 62));

        foreach (var row in rows)
        {
            Console.WriteLine(
                $"{row.CustomerName,-20} " +
                $"{row.OrderCount,10} " +
                $"{row.TotalAmount,14:F2} " +
                $"{row.AverageAmount,14:F2}");
        }
    }

    private static void PrintTopReports(
        IEnumerable<Order> orders)
    {
        // Матеріалізуємо джерело один раз для двох звітів.
        var source = orders.ToList();

        var customers =
            AdditionalReportService.TopCustomersWithExtension(
                source,
                5);

        Console.WriteLine("ТОП-5 КЛІЄНТІВ ЧЕРЕЗ TOPBY");
        Console.WriteLine();

        Console.WriteLine(
            $"{"Клієнт",-20} " +
            $"{"Замовлень",10} " +
            $"{"Сума, грн",14}");

        foreach (var customer in customers)
        {
            Console.WriteLine(
                $"{customer.CustomerName,-20} " +
                $"{customer.OrderCount,10} " +
                $"{customer.TotalAmount,14:F2}");
        }

        var topOrders =
            AdditionalReportService.TopOrdersWithExtension(
                source,
                5);

        Console.WriteLine();
        Console.WriteLine("ТОП-5 ЗАМОВЛЕНЬ ЧЕРЕЗ TOPBY");
        Console.WriteLine();

        Console.WriteLine(
            $"{"ID",-14} " +
            $"{"Клієнт",-20} " +
            $"{"Сума, грн",14}");

        foreach (var order in topOrders)
        {
            Console.WriteLine(
                $"{order.Id,-14} " +
                $"{order.CustomerName,-20} " +
                $"{order.Total,14:F2}");
        }
    }

    private static void PrintQueryReport(
        IEnumerable<Order> orders)
    {
        var source = orders.ToList();

        var queryResult =
            AdditionalReportService.ByStatusQuerySyntax(source);

        var methodResult =
            ReportService.ByStatus(source);

        Console.WriteLine(
            "ГРУПУВАННЯ У СИНТАКСИСІ ЗАПИТІВ");

        Console.WriteLine();

        Console.WriteLine(
            $"{"Статус",-20} " +
            $"{"Замовлень",10} " +
            $"{"Сума, грн",14}");

        foreach (var row in queryResult)
        {
            Console.WriteLine(
                $"{row.Status,-20} " +
                $"{row.OrderCount,10} " +
                $"{row.TotalAmount,14:F2}");
        }

        Console.WriteLine();

        Console.WriteLine(
            "Результати двох синтаксисів однакові: " +
            queryResult.SequenceEqual(methodResult));
    }

    private static void PrintBenchmark()
    {
        var result = AdditionalReportService.Benchmark(
            count: 100_000,
            repetitions: 5);

        Console.WriteLine("ПОРІВНЯННЯ ЧАСУ LINQ-ЗАПИТІВ");
        Console.WriteLine();

        Console.WriteLine(
            $"Кількість замовлень: {result.SourceCount}");

        Console.WriteLine(
            $"Кількість повторень: {result.Repetitions}");

        Console.WriteLine(
            $"Кількість рядків звіту: {result.ResultCount}");

        Console.WriteLine(
            "Середній час без проміжного ToList: " +
            $"{result.WithoutIntermediateListMs:F3} мс");

        Console.WriteLine(
            "Середній час із проміжним ToList: " +
            $"{result.WithIntermediateListMs:F3} мс");

        Console.WriteLine(
            $"Результати однакові: {result.ResultsEqual}");

        Console.WriteLine();
        Console.WriteLine(
            "Генерація даних не входить у виміряний час.");

        Console.WriteLine(
            "Час залежить від комп’ютера та навантаження.");
    }
}