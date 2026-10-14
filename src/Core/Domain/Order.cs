using System;
using System.Collections.Generic;
using System.Linq;
using Core.Dto;

namespace Core.Domain;

public sealed class Order
{
    private readonly List<OrderLine> _lines = new();

    public string Id { get; }
    public string CustomerId { get; }

    public bool IsConfirmed { get; private set; }

    public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();

    public decimal Total => _lines.Sum(line => line.Subtotal);

    private Order(string id, string customerId)
    {
        Id = id;
        CustomerId = customerId;
    }

    public static Order Create(string id, string customerId)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException(
                "Номер замовлення не може бути порожнім.",
                nameof(id));
        }

        if (string.IsNullOrWhiteSpace(customerId))
        {
            throw new ArgumentException(
                "Ідентифікатор клієнта не може бути порожнім.",
                nameof(customerId));
        }

        return new Order(id.Trim(), customerId.Trim());
    }

    public void AddLine(
        string productId,
        string name,
        decimal price,
        int quantity)
    {
        if (IsConfirmed)
        {
            throw new InvalidOperationException(
                $"Замовлення {Id} вже підтверджене. " +
                "Додавати рядки не можна.");
        }

        OrderLine line = OrderLine.Create(
            productId,
            name,
            price,
            quantity);

        if (line.Subtotal > decimal.MaxValue - Total)
        {
            throw new InvalidOperationException(
                $"Додавання рядка перевищить допустиму суму " +
                $"замовлення {Id}.");
        }

        _lines.Add(line);
    }

    public void Confirm()
    {
        if (IsConfirmed)
        {
            throw new InvalidOperationException(
                $"Замовлення {Id} вже підтверджене.");
        }

        if (_lines.Count == 0)
        {
            throw new InvalidOperationException(
                $"Не можна підтвердити порожнє замовлення {Id}.");
        }

        IsConfirmed = true;
    }

    public OrderDto ToDto()
    {
        var lines = _lines
            .Select(line => line.ToDto())
            .ToList()
            .AsReadOnly();

        return new OrderDto(
            Id,
            CustomerId,
            IsConfirmed,
            lines);
    }

    public static Order FromDto(OrderDto dto)
    {
        if (dto is null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        if (dto.Lines is null)
        {
            throw new ArgumentException(
                "Колекція рядків замовлення не може бути null.",
                nameof(dto));
        }

        Order order = Create(dto.Id, dto.CustomerId);

        foreach (OrderLineDto lineDto in dto.Lines)
        {
            OrderLine line = OrderLine.FromDto(lineDto);

            order.AddLine(
                line.ProductId,
                line.Name,
                line.Price,
                line.Quantity);
        }

        if (dto.IsConfirmed)
        {
            
        }

        return order;
    }

    public override string ToString()
    {
        string status = IsConfirmed
            ? "Підтверджене"
            : "Чернетка";

        return $"{Id}; клієнт: {CustomerId}; " +
               $"статус: {status}; рядків: {Lines.Count}; " +
               $"сума: {Total:F2} грн";
    }
}