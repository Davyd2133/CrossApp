using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Order = Core.Domain.Order;

namespace CrossApp.Core.Reports;

public static class AdditionalReportService
{
    // ========================================================
    // 1. GROUPJOIN ДВОХ КОЛЕКЦІЙ
    // ========================================================

    public static IReadOnlyList<CustomerOrdersSummaryDto>
        CustomersWithOrders(
            IEnumerable<ReportCustomer> customers,
            IEnumerable<Order> orders)
    {
        ArgumentNullException.ThrowIfNull(customers);
        ArgumentNullException.ThrowIfNull(orders);

        return customers
            .GroupJoin(
                orders,
                customer => customer.Name,
                order => order.CustomerName,
                (customer, customerOrders) =>
                {
                    var list = customerOrders.ToList();

                    return new CustomerOrdersSummaryDto(
                        CustomerName: customer.Name,
                        OrderCount: list.Count,
                        TotalAmount: list.Sum(order => order.Total),
                        AverageAmount: list.Count == 0
                            ? 0m
                            : list.Average(order => order.Total));
                },
                StringComparer.Ordinal)
            .OrderBy(
                row => row.CustomerName,
                StringComparer.Ordinal)
            .ToList();
    }

    // ========================================================
    // 2. ВЛАСНИЙ TOPBY У ДВОХ ЗВІТАХ
    // ========================================================

    // Перший звіт: топ клієнтів за сумою всіх замовлень.
    public static IReadOnlyList<CustomerTotalDto>
        TopCustomersWithExtension(
            IEnumerable<Order> source,
            int n = 5)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source
            .GroupBy(
                order => order.CustomerName,
                StringComparer.Ordinal)
            .Select(group => new CustomerTotalDto(
                CustomerName: group.Key,
                OrderCount: group.Count(),
                TotalAmount: group.Sum(order => order.Total)))
            .TopBy(
                valueSelector: row => row.TotalAmount,
                tieBreaker: row => row.CustomerName,
                n: n);
    }

    // Другий звіт: топ окремих замовлень за сумою.
    public static IReadOnlyList<OrderReportDto>
        TopOrdersWithExtension(
            IEnumerable<Order> source,
            int n = 5)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source
            .Select(order => new OrderReportDto(
                Id: order.Id,
                CustomerName: order.CustomerName,
                Status: order.Status.ToString(),
                Total: order.Total))
            .TopBy(
                valueSelector: row => row.Total,
                tieBreaker: row => row.Id,
                n: n);
    }

    // ========================================================
    // 3. СИНТАКСИС ЗАПИТІВ
    // ========================================================

   public static IReadOnlyList<StatusSummaryDto>
        ByStatusQuerySyntax(
            IEnumerable<Order> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var query =
            from order in source
            group order by order.Status
            into statusGroup
            select new StatusSummaryDto(
                Status: statusGroup.Key.ToString(),
                OrderCount: statusGroup.Count(),
                TotalAmount: statusGroup.Sum(order => order.Total));

        return query
            .OrderBy(
                row => row.Status,
                StringComparer.Ordinal)
            .ToList();
    }
    
    // ========================================================
    // 4. ВИМІРЮВАННЯ ЧАСУ
    // ========================================================

    public static BenchmarkResultDto Benchmark(
        int count = 100_000,
        int repetitions = 5)
    {
        if (count <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(count),
                "Кількість замовлень має бути більшою за нуль.");
        }

        if (repetitions <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(repetitions),
                "Кількість повторень має бути більшою за нуль.");
        }

        // Генерація виконується поза вимірюванням.
        var orders = GenerateOrders(count);

        // Прогрів обох варіантів.
        var expected = QueryWithoutIntermediateList(orders);
        var actual = QueryWithIntermediateList(orders);

        bool resultsEqual = expected.SequenceEqual(actual);

        if (!resultsEqual)
        {
            throw new InvalidOperationException(
                "Варіанти запиту повернули різні результати.");
        }

        double withoutListMs = 0;
        double withListMs = 0;

        // Чергуємо порядок, щоб зменшити вплив черговості.
        for (int i = 0; i < repetitions; i++)
        {
            if (i % 2 == 0)
            {
                withoutListMs += Measure(
                    () => QueryWithoutIntermediateList(orders));

                withListMs += Measure(
                    () => QueryWithIntermediateList(orders));
            }
            else
            {
                withListMs += Measure(
                    () => QueryWithIntermediateList(orders));

                withoutListMs += Measure(
                    () => QueryWithoutIntermediateList(orders));
            }
        }

        return new BenchmarkResultDto(
            SourceCount: count,
            Repetitions: repetitions,
            ResultCount: expected.Count,
            WithoutIntermediateListMs:
                withoutListMs / repetitions,
            WithIntermediateListMs:
                withListMs / repetitions,
            ResultsEqual: resultsEqual);
    }

    private static List<Order> GenerateOrders(int count)
    {
        var random = new Random(3);

        var orders = new List<Order>(count);

        for (int i = 0; i < count; i++)
        {
            var order = Order.Create(
                $"BENCH-{i:D6}",
                $"Клієнт-{i % 1000:D4}");

            int quantity = random.Next(1, 6);
            decimal unitPrice = random.Next(100, 5001);

            order.AddLine(
                "Тестовий товар",
                quantity,
                unitPrice);

            if (i % 2 == 0)
            {
                order.Confirm();
            }

            orders.Add(order);
        }

        return orders;
    }

    // Один ланцюжок без проміжної матеріалізації.
    private static List<OrderReportDto>
        QueryWithoutIntermediateList(
            IEnumerable<Order> source)
    {
        return source
            .Where(order => order.Total > 5000m)
            .OrderByDescending(order => order.Total)
            .ThenBy(
                order => order.Id,
                StringComparer.Ordinal)
            .Take(100)
            .Select(order => new OrderReportDto(
                Id: order.Id,
                CustomerName: order.CustomerName,
                Status: order.Status.ToString(),
                Total: order.Total))
            .ToList();
    }

    // Той самий запит, але з ToList після Where.
    private static List<OrderReportDto>
        QueryWithIntermediateList(
            IEnumerable<Order> source)
    {
        return source
            .Where(order => order.Total > 5000m)
            .ToList()
            .OrderByDescending(order => order.Total)
            .ThenBy(
                order => order.Id,
                StringComparer.Ordinal)
            .Take(100)
            .Select(order => new OrderReportDto(
                Id: order.Id,
                CustomerName: order.CustomerName,
                Status: order.Status.ToString(),
                Total: order.Total))
            .ToList();
    }

    private static double Measure(
        Func<List<OrderReportDto>> action)
    {
        var stopwatch = Stopwatch.StartNew();

        var result = action();

        stopwatch.Stop();

        GC.KeepAlive(result);

        return stopwatch.Elapsed.TotalMilliseconds;
    }
}