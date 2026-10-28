using System;

namespace Core.Domain;

public sealed class Customer
{
    public string Id { get; }

    public bool IsOrderingBlocked { get; private set; }

    private Customer(string id)
    {
        Id = id;
    }

    public static Customer Create(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException(
                "Ідентифікатор клієнта не може бути порожнім.",
                nameof(id));
        }

        return new Customer(id.Trim());
    }

    public void BlockOrdering()
    {
        IsOrderingBlocked = true;
    }

    public void UnblockOrdering()
    {
        IsOrderingBlocked = false;
    }
}