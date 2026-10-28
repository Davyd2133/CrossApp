using Core.Domain;

namespace Core;

public static class SampleData
{
    public static IEnumerable<Order> Orders()
    {
        var orders = new List<Order>();

        for (int i = 1; i <= 15; i++)
        {
            var order = Order.Create(
                $"ORD-{i:000}",
                $"Замовник {i}");

            order.AddLine(
                "Клавіатура",
                1,
                800m);

            order.AddLine(
                "Миша",
                2,
                400m);

            if (i % 2 == 0)
            {
                order.Confirm();
            }

            orders.Add(order);
        }

        return orders;
    }
}