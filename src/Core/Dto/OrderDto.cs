using Core.Domain;

namespace Core.Dto;

public sealed record OrderDto(
    string Id,
    string CustomerName,
    DateTime CreatedAt,
    OrderStatus Status,
    List<OrderLineDto> Lines
);