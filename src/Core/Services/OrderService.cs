using System;
using System.Collections.Generic;
using System.Linq;
using Core.Entities;
using Core.Events;
using Core.Interfaces;

namespace Core.Services;

public sealed class OrderService
{
    private readonly IOrderStore _store;

    public OrderService(IOrderStore store)
    {
        _store = store
            ?? throw new ArgumentNullException(nameof(store));
    }

    public event EventHandler<OrderChangedEventArgs>? Changed;

    private void OnChanged(
        Order order,
        OrderChangeKind kind)
    {
        Changed?.Invoke(
            this,
            new OrderChangedEventArgs(
                order.Id,
                kind,
                order.LinesCount,
                order.Total));
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
}