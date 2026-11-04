namespace CrossApp.Core.Reports;

public sealed record OrderTotalsDto(
    int OrderCount,
    decimal TotalAmount,
    decimal AverageAmount,
    decimal MaxAmount);