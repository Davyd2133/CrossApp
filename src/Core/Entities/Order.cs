using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Entities;

public sealed class Order
{
    private readonly List<OrderLine> _lines = new();

    public string Id { get; }
    public bool IsConfirmed { get; private set; }

    public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();

    public int LinesCount => _lines.Count;

    public decimal Total => _lines.Sum(line => line.Total);

    private Order(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException(
                "Ідентифікатор не може бути порожнім.",
                nameof(id));
        }

        Id = id;
    }

    public static Order Create(string id)
    {
        return new Order(id);
    }

    public void AddLine(
        string productName,
        decimal unitPrice,
        int quantity)
    {
        if (IsConfirmed)
        {
            throw new InvalidOperationException(
                "Не можна додавати позиції до підтвердженого замовлення.");
        }

        var line = new OrderLine(
            productName,
            unitPrice,
            quantity);

        // Перевіряємо підсумкову суму до зміни списку.
        _ = checked(Total + line.Total);

        _lines.Add(line);
    }

    public void Confirm()
    {
        if (IsConfirmed)
        {
            throw new InvalidOperationException(
                "Замовлення вже підтверджено.");
        }

        if (_lines.Count == 0)
        {
            throw new InvalidOperationException(
                "Не можна підтвердити порожнє замовлення.");
        }

        IsConfirmed = true;
    }

    public Order Copy()
    {
        var copy = new Order(Id);

        // OrderLine незмінний, тому позиції можна безпечно розділяти.
        copy._lines.AddRange(_lines);
        copy.IsConfirmed = IsConfirmed;

        return copy;
    }
}