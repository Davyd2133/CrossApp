using System.Collections.Generic;
using Order = Core.Domain.Order;

namespace CrossApp.Core.Reports;

public static class ReportSampleData
{
    public static IReadOnlyList<Order> CreateOrders()
    {
        return new List<Order>
        {
            CreateOrder(
                "ORD-001", "Давид",
                "Клавіатура", 1, 1200m, true),

            CreateOrder(
                "ORD-002", "Олена",
                "Миша", 2, 400m, false),

            CreateOrder(
                "ORD-003", "Андрій",
                "Монітор", 1, 2500m, true),

            CreateOrder(
                "ORD-004", "Марія",
                "Блокнот", 3, 150m, false),

            CreateOrder(
                "ORD-005", "Давид",
                "Навушники", 1, 1800m, true),

            CreateOrder(
                "ORD-006", "Олена",
                "Принтер", 1, 2200m, true),

            CreateOrder(
                "ORD-007", "Іван",
                "Флешка", 3, 300m, false),

            CreateOrder(
                "ORD-008", "Софія",
                "Планшет", 1, 3100m, true),

            CreateOrder(
                "ORD-009", "Андрій",
                "Кабель", 2, 350m, false),

            CreateOrder(
                "ORD-010", "Марія",
                "Колонки", 1, 1550m, true),

            CreateOrder(
                "ORD-011", "Іван",
                "Крісло", 1, 2100m, true),

            CreateOrder(
                "ORD-012", "Софія",
                "Килимок", 2, 200m, false),

            CreateOrder(
                "ORD-013", "Давид",
                "Зарядний пристрій", 1, 600m, false),

            CreateOrder(
                "ORD-014", "Олена",
                "Вебкамера", 1, 1000m, true),

            CreateOrder(
                "ORD-015", "Андрій",
                "Мікрофон", 1, 1300m, true),

            CreateOrder(
                "ORD-016", "Марія",
                "Папір", 5, 100m, false),

            CreateOrder(
                "ORD-017", "Іван",
                "Настільна лампа", 1, 750m, false),

            CreateOrder(
                "ORD-018", "Софія",
                "Роутер", 1, 1250m, true)
        };
    }

    private static Order CreateOrder(
        string id,
        string customerName,
        string productName,
        int quantity,
        decimal unitPrice,
        bool confirmed)
    {
        var order = Order.Create(
            id,
            customerName);

        order.AddLine(
            productName,
            quantity,
            unitPrice);

        if (confirmed)
        {
            order.Confirm();
        }

        return order;
    }
}