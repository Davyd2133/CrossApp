namespace Core.Domain;

public sealed class OrderLine
{
    public string ProductName { get; }

    public int Quantity { get; }

    public decimal UnitPrice { get; }

    public decimal Total => Quantity * UnitPrice;

    private OrderLine(
        string productName,
        int quantity,
        decimal unitPrice)
    {
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public static OrderLine Create(
        string productName,
        int quantity,
        decimal unitPrice)
    {
        if (string.IsNullOrWhiteSpace(productName))
        {
            throw new ArgumentException(
                "Назва товару не може бути порожньою.",
                nameof(productName));
        }

        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantity),
                "Кількість має бути більшою за нуль.");
        }

        if (unitPrice < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(unitPrice),
                "Ціна не може бути від’ємною.");
        }

        return new OrderLine(
            productName.Trim(),
            quantity,
            unitPrice);
    }
}