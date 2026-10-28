using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Core.Entities;
using Core.Interfaces;

namespace Cli.Stores;

public sealed class JsonOrderStore : IOrderStore
{
    private readonly string _path;

    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true
    };

    public JsonOrderStore(string path)
    {
        _path = Path.GetFullPath(path);

        Directory.CreateDirectory(
            Path.GetDirectoryName(_path)!);
    }

    public void Add(Order order)
    {
        var orders = ReadOrders();

        if (orders.Any(item => item.Id == order.Id))
        {
            throw new InvalidOperationException(
                $"Замовлення з id={order.Id} вже існує.");
        }

        orders.Add(order.Copy());
        SaveOrders(orders);
    }

    public Order? GetById(string id)
    {
        return ReadOrders()
            .FirstOrDefault(order => order.Id == id);
    }

    public void Update(Order order)
    {
        var orders = ReadOrders();

        int index = orders.FindIndex(
            item => item.Id == order.Id);

        if (index < 0)
        {
            throw new InvalidOperationException(
                $"Замовлення з id={order.Id} не знайдено.");
        }

        orders[index] = order.Copy();
        SaveOrders(orders);
    }

    public IReadOnlyList<Order> List()
    {
        return ReadOrders();
    }

    private List<Order> ReadOrders()
    {
        if (!File.Exists(_path))
        {
            return new List<Order>();
        }

        string json = File.ReadAllText(_path);

        var data = JsonSerializer.Deserialize<List<OrderData>>(
            json,
            _options)
            ?? throw new InvalidDataException(
                "Не вдалося прочитати замовлення з JSON.");

        return data.Select(RestoreOrder).ToList();
    }

    private void SaveOrders(IEnumerable<Order> orders)
    {
        var data = orders.Select(order => new OrderData
        {
            Id = order.Id,
            IsConfirmed = order.IsConfirmed,

            Lines = order.Lines.Select(line => new LineData
            {
                ProductName = line.ProductName,
                UnitPrice = line.UnitPrice,
                Quantity = line.Quantity
            }).ToList()
        }).ToList();

        string json = JsonSerializer.Serialize(data, _options);

        File.WriteAllText(_path, json);
    }

    private static Order RestoreOrder(OrderData data)
    {
        var order = Order.Create(data.Id);

        foreach (var line in data.Lines)
        {
            order.AddLine(
                line.ProductName,
                line.UnitPrice,
                line.Quantity);
        }

        if (data.IsConfirmed)
        {
            order.Confirm();
        }

        return order;
    }

    public sealed class OrderData
    {
        public string Id { get; set; } = "";
        public bool IsConfirmed { get; set; }

        public List<LineData> Lines { get; set; } = new();
    }

    public sealed class LineData
    {
        public string ProductName { get; set; } = "";
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
    }
}