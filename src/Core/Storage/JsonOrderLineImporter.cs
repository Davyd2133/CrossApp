using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Core.Entities;

namespace Core.Storage;

public sealed class JsonOrderLineImporter : global::IOrderLineImporter
{
    public async Task<IReadOnlyList<OrderLine>> LoadAsync(
        string path,
        CancellationToken ct = default)
    {
        var lines = new List<OrderLine>();

        await foreach (var line in StreamAsync(path, ct))
        {
            lines.Add(line);
        }

        return lines;
    }

    public async IAsyncEnumerable<OrderLine> StreamAsync(
        string path,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            useAsync: true);

        int recordNumber = 0;

        await foreach (var record in
            JsonSerializer.DeserializeAsyncEnumerable<OrderLineDto>(
                stream, cancellationToken: ct))
        {
            ct.ThrowIfCancellationRequested();
            recordNumber++;

            yield return ToOrderLine(record, recordNumber);
        }
    }

    private static OrderLine ToOrderLine(
        OrderLineDto? record,
        int recordNumber)
    {
        if (record is null || string.IsNullOrWhiteSpace(record.ProductName))
        {
            throw new global::Core.DomainException(
                $"Запис {recordNumber}: назва товару обов’язкова.");
        }

        try
        {
            return new OrderLine(
                record.ProductName,
                record.UnitPrice,
                record.Quantity);
        }
        catch (ArgumentException ex)
        {
            throw new global::Core.DomainException(
                $"Запис {recordNumber}: {ex.Message}", ex);
        }
        catch (OverflowException ex)
        {
            throw new global::Core.DomainException(
                $"Запис {recordNumber}: сума рядка завелика.", ex);
        }
    }

    private sealed record OrderLineDto(
        string? ProductName,
        decimal UnitPrice,
        int Quantity);
}