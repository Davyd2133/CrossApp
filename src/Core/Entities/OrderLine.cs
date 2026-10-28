namespace Core.Entities;

public sealed class OrderLine
{
    public string ProductName { get; }
    public decimal UnitPrice { get; }
    public int Quantity { get; }

    public decimal Total => UnitPrice * Quantity;

    public OrderLine(
        string productName,
        decimal unitPrice,
        int quantity)
    {
        if (string.IsNullOrWhiteSpace(productName))
        {
            throw new ArgumentException(
                "Назва товару не може бути порожньою.",
                nameof(productName));
        }

        if (unitPrice <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(unitPrice),
                "Ціна повинна бути більшою за нуль.");
        }

        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantity),
                "Кількість повинна бути більшою за нуль.");
        }

        // Перевіряємо переповнення до створення позиції.
        _ = checked(unitPrice * quantity);

        ProductName = productName.Trim();
        UnitPrice = unitPrice;
        Quantity = quantity;
    }
}