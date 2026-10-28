namespace Core.Dto;

public sealed record OrderLineDto(
    string ProductName,
    int Quantity,
    decimal UnitPrice
);