namespace CrossApp.Core.Reports;

public sealed record CustomerTotalDto(
    string CustomerName,
    int OrderCount,
    decimal TotalAmount);