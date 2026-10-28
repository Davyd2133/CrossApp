using System.Collections.Generic;
using Core.Domain;

namespace Core.Import;

public sealed record OrderDomainImportResult(
    IReadOnlyList<Order> Orders,
    IReadOnlyList<string> Errors);