using System;
using System.Text;
using Core.Domain;
using Core.Dto;

Console.OutputEncoding = Encoding.UTF8;

Console.WriteLine("=== Сценарій 1: успіх ===");

Order order = Order.Create("ORD-001", "C-001");

Console.WriteLine($"Створено: {order}");

order.AddLine("P-001", "Клавіатура", 800m, 2);
order.AddLine("P-002", "Миша", 400m, 1);

Console.WriteLine();
PrintOrder(order);

order.Confirm();

Console.WriteLine();
Console.WriteLine($"Після підтвердження: {order}");

Console.WriteLine();
Console.WriteLine("=== Перетворення в DTO та назад ===");

OrderDto dto = order.ToDto();
Order restored = Order.FromDto(dto);

PrintOrder(restored);

Console.WriteLine();
Console.WriteLine("=== Сценарій 2: порушення інваріантів ===");

int linesBefore = order.Lines.Count;
decimal totalBefore = order.Total;
bool confirmedBefore = order.IsConfirmed;

TryDo(
    "Додавання до підтвердженого замовлення",
    () => order.AddLine("P-003", "Монітор", 5000m, 1));

TryDo(
    "Повторне підтвердження",
    () => order.Confirm());

TryDo(
    "Порожній номер",
    () => Order.Create(" ", "C-002"));

Order draft = Order.Create("ORD-002", "C-002");

TryDo(
    "Нульова кількість",
    () => draft.AddLine("P-004", "Навушники", 600m, 0));

TryDo(
    "Від'ємна ціна",
    () => draft.AddLine("P-004", "Навушники", -600m, 1));

TryDo(
    "Порожній ідентифікатор товару",
    () => draft.AddLine(" ", "Навушники", 600m, 1));

TryDo(
    "Підтвердження порожнього замовлення",
    () => draft.Confirm());

TryDo(
    "Відновлення підтвердженого замовлення без рядків",
    () => Order.FromDto(
        new OrderDto(
            "ORD-003",
            "C-003",
            true,
            Array.Empty<OrderLineDto>())));

Console.WriteLine();
Console.WriteLine("=== Перевірка стану після відмов ===");

Console.WriteLine(
    $"Рядків: було {linesBefore}, стало {order.Lines.Count}");

Console.WriteLine(
    $"Сума: була {totalBefore:F2}, стала {order.Total:F2}");

Console.WriteLine(
    $"Підтвердження: було {confirmedBefore}, " +
    $"стало {order.IsConfirmed}");

Console.WriteLine(
    $"Чернетка: рядків {draft.Lines.Count}, " +
    $"підтверджена {draft.IsConfirmed}");

static void PrintOrder(Order order)
{
    Console.WriteLine(order);

    foreach (OrderLine line in order.Lines)
    {
        Console.WriteLine(
            $"  {line.ProductId}: {line.Name}; " +
            $"{line.Quantity} × {line.Price:F2} грн " +
            $"= {line.Subtotal:F2} грн");
    }
}

static void TryDo(string title, Action action)
{
    try
    {
        action();

        Console.WriteLine(
            $"{title}: очікуваний виняток НЕ виник!");
    }
    catch (Exception ex)
    {
        Console.WriteLine(
            $"{title}: {ex.GetType().Name} — {ex.Message}");
    }
}