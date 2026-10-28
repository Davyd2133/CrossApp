using System.Collections.Generic;
using Core.Entities;

namespace Core.Interfaces;

public interface IOrderStore
{
    void Add(Order order);

    Order? GetById(string id);

    void Update(Order order);

    IReadOnlyList<Order> List();
}