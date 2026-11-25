using System;
using System.Collections.Generic;
using System.Linq;
using Core.Entities;
using Core.Events;
using Core.Interfaces;
using System.Globalization;

namespace Core.Services;

public sealed class OrderService
{
    private readonly IOrderStore _store;
    private readonly INotificationSink? _notificationSink;

    public OrderService(
        IOrderStore store,
        INotificationSink? notificationSink = null)
    {
        _store = store
            ?? throw new ArgumentNullException(nameof(store));

        _notificationSink = notificationSink;
    }

    public event EventHandler<OrderChangedEventArgs>? Changed;

    private void OnChanged(
        Order order,
        OrderChangeKind kind)
    {
        var notification = new OrderChangedEventArgs(
            order.Id,
            kind,
            order.LinesCount,
            order.Total);

        _notificationSink?.Notify(notification);

        Changed?.Invoke(this, notification);
    }

    public Order Create()
    {
        var order = Order.Create(
            Guid.NewGuid().ToString("N"));

        _store.Add(order);

        OnChanged(order, OrderChangeKind.OrderCreated);

        return order;
    }

    public void AddLine(
        string orderId,
        string productName,
        decimal unitPrice,
        int quantity)
    {
        var order = GetRequired(orderId);

        order.AddLine(
            productName,
            unitPrice,
            quantity);

        _store.Update(order);

        OnChanged(order, OrderChangeKind.LineAdded);
    }

    public async Task<int> ImportAsync(
    string orderId,
    global::IOrderLineImporter importer,
    string path,
    CancellationToken ct = default,
    IProgress<int>? progress = null,
    int totalLines = 0)
{
    ArgumentNullException.ThrowIfNull(importer);
    ct.ThrowIfCancellationRequested();

    int added = 0;
    progress?.Report(0);

    await foreach (var line in importer.StreamAsync(path, ct))
    {
        ct.ThrowIfCancellationRequested();

        AddLine(
            orderId,
            line.ProductName,
            line.UnitPrice,
            line.Quantity);

        added++;

        if (totalLines > 0)
        {
            int percent = (int)Math.Min(99L, (long)added * 100 / totalLines);
            progress?.Report(percent);
        }
    }

    ct.ThrowIfCancellationRequested();
    progress?.Report(100);

    return added;
}

    public void Confirm(string orderId)
    {
        var order = GetRequired(orderId);

        order.Confirm();

        _store.Update(order);

        OnChanged(order, OrderChangeKind.OrderConfirmed);
    }

    public Order? Find(string orderId)
    {
        return _store.GetById(orderId);
    }

    public IReadOnlyList<Order> FindAll(
        Func<Order, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        return _store.List()
            .Where(predicate)
            .ToList();
    }

    private Order GetRequired(string orderId)
    {
        return _store.GetById(orderId)
            ?? throw new InvalidOperationException(
                $"Замовлення з id={orderId} не знайдено.");
    }

    public int ImportLines(
        string orderId,
        IEnumerable<string> rows,
        Action<string> reportSkipped)
    {
        ArgumentNullException.ThrowIfNull(rows);
        ArgumentNullException.ThrowIfNull(reportSkipped);

        var order = GetRequired(orderId);

        if (order.IsConfirmed)
        {
            throw new InvalidOperationException(
                "Не можна імпортувати позиції " +
                "до підтвердженого замовлення.");
        }

        int imported = 0;
        int lineNumber = 0;

        foreach (string? row in rows)
        {
            lineNumber++;

            if (string.IsNullOrWhiteSpace(row))
            {
                reportSkipped(
                    $"Рядок {lineNumber}: порожній рядок.");

                continue;
            }

            string[] parts = row.Split(';');

            if (parts.Length != 3)
            {
                reportSkipped(
                    $"Рядок {lineNumber}: " +
                    "потрібний формат Назва;Ціна;Кількість.");

                continue;
            }

            string name = parts[0].Trim();

            if (!decimal.TryParse(
                    parts[1].Trim(),
                    NumberStyles.AllowLeadingSign |
                    NumberStyles.AllowDecimalPoint,
                    CultureInfo.InvariantCulture,
                    out decimal price))
            {
                reportSkipped(
                    $"Рядок {lineNumber}: неправильна ціна.");

                continue;
            }

            if (!int.TryParse(
                    parts[2].Trim(),
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out int quantity))
            {
                reportSkipped(
                    $"Рядок {lineNumber}: неправильна кількість.");

                continue;
            }

            // Перевірка без зміни збереженого замовлення.
            var candidate = GetRequired(orderId);

            try
            {
                candidate.AddLine(name, price, quantity);
            }
            catch (ArgumentException ex)
            {
                reportSkipped(
                    $"Рядок {lineNumber}: {ex.Message}");

                continue;
            }
            catch (OverflowException)
            {
                reportSkipped(
                    $"Рядок {lineNumber}: надто велика сума.");

                continue;
            }

            AddLine(orderId, name, price, quantity);

            imported++;
        }

        return imported;
    }

}