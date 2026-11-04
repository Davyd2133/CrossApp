namespace CrossApp.Core.Reports;

public sealed record StatusSummaryDto(
    string Status,
    int OrderCount,
    decimal TotalAmount);