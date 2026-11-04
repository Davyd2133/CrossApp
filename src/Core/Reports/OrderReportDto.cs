namespace CrossApp.Core.Reports;

public sealed record OrderReportDto(
    string Id,
    string CustomerName,
    string Status,
    decimal Total);