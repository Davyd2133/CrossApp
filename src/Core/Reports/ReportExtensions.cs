using System;
using System.Collections.Generic;
using System.Linq;

namespace CrossApp.Core.Reports;

public static class ReportExtensions
{
    public static IReadOnlyList<T> TopBy<T>(
        this IEnumerable<T> source,
        Func<T, decimal> valueSelector,
        Func<T, string> tieBreaker,
        int n)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(valueSelector);
        ArgumentNullException.ThrowIfNull(tieBreaker);

        if (n < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(n),
                "Кількість елементів не може бути від’ємною.");
        }

        return source
            .OrderByDescending(valueSelector)
            .ThenBy(tieBreaker, StringComparer.Ordinal)
            .Take(n)
            .ToList();
    }
}