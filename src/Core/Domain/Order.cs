using Core.Dto;

namespace Core.Domain;

public sealed class Order
{
    private readonly List<OrderLine> _lines = new();

    public string Id { get; }

    public string CustomerName { get; }

    public DateTime CreatedAt { get; }

    public OrderStatus Status { get; private set; }

    public IReadOnlyList<OrderLine> Lines =>
        _lines.AsReadOnly();

    public decimal Total =>
        _lines.Sum(line => line.Total);

    private Order(
        string id,
        string customerName,
        DateTime createdAt)
    {
        Id = id;
        CustomerName = customerName;
        CreatedAt = createdAt;
        Status = OrderStatus.Draft;
    }

    public static Order Create(
        string id,
        string customerName)
    {
        ValidateHeader(id, customerName);

        return new Order(
            id.Trim(),
            customerName.Trim(),
            DateTime.UtcNow);
    }

    private static void ValidateHeader(
        string id,
        string customerName)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException(
                "Ідентифікатор замовлення не може бути порожнім.",
                nameof(id));
        }

        if (string.IsNullOrWhiteSpace(customerName))
        {
            throw new ArgumentException(
                "Ім’я замовника не може бути порожнім.",
                nameof(customerName));
        }
    }

    public void AddLine(
        string productName,
        int quantity,
        decimal unitPrice)
    {
        if (Status != OrderStatus.Draft)
        {
            throw new InvalidOperationException(
                "Не можна додавати товари до підтвердженого замовлення.");
        }

        OrderLine line = OrderLine.Create(
            productName,
            quantity,
            unitPrice);

        _lines.Add(line);
    }

    public void Confirm()
    {
        if (Status == OrderStatus.Confirmed)
        {
            throw new InvalidOperationException(
                "Замовлення вже підтверджено.");
        }

        if (_lines.Count == 0)
        {
            throw new InvalidOperationException(
                "Не можна підтвердити замовлення без товарів.");
        }

        Status = OrderStatus.Confirmed;
    }

    public OrderDto ToDto()
    {
        var lines = _lines
            .Select(line => new OrderLineDto(
                line.ProductName,
                line.Quantity,
                line.UnitPrice))
            .ToList();

        return new OrderDto(
            Id,
            CustomerName,
            CreatedAt,
            Status,
            lines);
    }

    public static Order FromDto(OrderDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        ValidateHeader(dto.Id, dto.CustomerName);

        if (!Enum.IsDefined(typeof(OrderStatus), dto.Status))
        {
            throw new ArgumentException(
                "Невідомий статус замовлення.");
        }

        if (dto.Lines is null)
        {
            throw new ArgumentException(
                "Список рядків замовлення відсутній.");
        }

        var order = new Order(
            dto.Id.Trim(),
            dto.CustomerName.Trim(),
            dto.CreatedAt);

        foreach (var line in dto.Lines)
        {
            ArgumentNullException.ThrowIfNull(line);

            order.AddLine(
                line.ProductName,
                line.Quantity,
                line.UnitPrice);
        }

        if (dto.Status == OrderStatus.Confirmed)
        {
            order.Confirm();
        }

        return order;
    }
}