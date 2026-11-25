using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Entities;

public interface IOrderLineImporter
{
    Task<IReadOnlyList<OrderLine>> LoadAsync(
        string path,
        CancellationToken ct = default);

    IAsyncEnumerable<OrderLine> StreamAsync(
        string path,
        CancellationToken ct = default);
}