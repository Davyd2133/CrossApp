namespace CrossApp.Core.Reports;

// Окрема колекція клієнтів для GroupJoin.
public sealed record ReportCustomer(
    string Name);

// Результат об’єднання клієнтів із замовленнями.
public sealed record CustomerOrdersSummaryDto(
    string CustomerName,
    int OrderCount,
    decimal TotalAmount,
    decimal AverageAmount);

// Результат вимірювання продуктивності.
public sealed record BenchmarkResultDto(
    int SourceCount,
    int Repetitions,
    int ResultCount,
    double WithoutIntermediateListMs,
    double WithIntermediateListMs,
    bool ResultsEqual);