using System;
using System.Collections.Generic;
using System.Linq;
using Core.Entities;
using Core.Interfaces;

namespace Cli.Stores;

public sealed class InMemoryOrderStore : IOrderStore
{
    private readonly Dictionary<string, Order> _orders = new();

    public void Add(Order order)
    {
        if (!_orders.TryAdd(order.Id, order.Copy()))
        {
            throw new InvalidOperationException(
                $"Замовлення з id={order.Id} вже існує.");
        }
    }

    public Order? GetById(string id)
    {
        return _orders.TryGetValue(id, out var order)
            ? order.Copy()
            : null;
    }

    public void Update(Order order)
    {
        if (!_orders.ContainsKey(order.Id))
        {
            throw new InvalidOperationException(
                $"Замовлення з id={order.Id} не знайдено.");
        }

        _orders[order.Id] = order.Copy();
    }

    public IReadOnlyList<Order> List()
    {
        return _orders.Values
            .Select(order => order.Copy())
            .ToList();
    }
}