using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class CachingOrderStore : IOrderStore
{
    private readonly IOrderStore _inner;

    private IReadOnlyList<Order>? _cachedOrders;

    public CachingOrderStore(IOrderStore inner)
    {
        _inner = inner
            ?? throw new ArgumentNullException(nameof(inner));
    }

    public IReadOnlyList<Order> List()
    {
        if (_cachedOrders is null)
        {
            _cachedOrders = _inner
                .List()
                .ToList()
                .AsReadOnly();
        }

        return _cachedOrders;
    }

    public Order? GetById(string id)
    {
        return _inner.GetById(id);
    }

    public void Add(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        InvalidateCache();

        _inner.Add(order);
    }

    public void Update(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        InvalidateCache();

        _inner.Update(order);
    }

    public bool Remove(string id)
    {
        InvalidateCache();

        return _inner.Remove(id);
    }

    private void InvalidateCache()
    {
        _cachedOrders = null;
    }
}