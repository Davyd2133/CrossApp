using System;
using System.Collections.Generic;
using System.Linq;
using Order = Core.Domain.Order;

namespace CrossApp.Core.Reports;

public static class ReportService
{
    // 1. Топ клієнтів за загальною сумою їхніх замовлень.
    public static IReadOnlyList<CustomerTotalDto> TopCustomers(
        IEnumerable<Order> source,
        int n = 5)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (n < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(n),
                "Кількість клієнтів не може бути від’ємною.");
        }

        return source
            .GroupBy(
                order => order.CustomerName,
                StringComparer.Ordinal)
            .Select(group => new CustomerTotalDto(
                CustomerName: group.Key,
                OrderCount: group.Count(),
                TotalAmount: group.Sum(order => order.Total)))
            .OrderByDescending(client => client.TotalAmount)
            .ThenBy(
                client => client.CustomerName,
                StringComparer.Ordinal)
            .Take(n)
            .ToList();
    }

    // 2. Замовлення, сума яких строго більша за поріг.
    public static IReadOnlyList<OrderReportDto> OrdersAbove(
        IEnumerable<Order> source,
        decimal threshold)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (threshold < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(threshold),
                "Поріг суми не може бути від’ємним.");
        }

        return source
            .Where(order => order.Total > threshold)
            .OrderByDescending(order => order.Total)
            .ThenBy(
                order => order.Id,
                StringComparer.Ordinal)
            .Select(order => new OrderReportDto(
                Id: order.Id,
                CustomerName: order.CustomerName,
                Status: order.Status.ToString(),
                Total: order.Total))
            .ToList();
    }

    // 3. Кількість і загальна сума замовлень за статусами.
    public static IReadOnlyList<StatusSummaryDto> ByStatus(
        IEnumerable<Order> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source
            .GroupBy(order => order.Status)
            .Select(group => new StatusSummaryDto(
                Status: group.Key.ToString(),
                OrderCount: group.Count(),
                TotalAmount: group.Sum(order => order.Total)))
            .OrderBy(
                summary => summary.Status,
                StringComparer.Ordinal)
            .ToList();
    }

    // 4. Загальна сума, середній чек і максимальне замовлення.
    public static OrderTotalsDto Totals(
        IEnumerable<Order> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var orders = source.ToList();

        if (orders.Count == 0)
        {
            return new OrderTotalsDto(
                OrderCount: 0,
                TotalAmount: 0m,
                AverageAmount: 0m,
                MaxAmount: 0m);
        }

        return new OrderTotalsDto(
            OrderCount: orders.Count,
            TotalAmount: orders.Sum(order => order.Total),
            AverageAmount: orders.Average(order => order.Total),
            MaxAmount: orders.Max(order => order.Total));
    }

    // Запит виконується під час перебирання, Count() або ToList().
    // Повторне виконання враховує актуальний вміст джерела.
    // ToList() фіксує значення DTO в окремому списку.
    public static IEnumerable<OrderReportDto> DeferredOrdersAbove(
        IEnumerable<Order> source,
        decimal threshold)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (threshold < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(threshold),
                "Поріг суми не може бути від’ємним.");
        }

        return source
            .Where(order => order.Total > threshold)
            .Select(order => new OrderReportDto(
                Id: order.Id,
                CustomerName: order.CustomerName,
                Status: order.Status.ToString(),
                Total: order.Total));
    }
}