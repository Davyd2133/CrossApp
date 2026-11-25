using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CrossApp.Lab09.Domain;

public sealed record OrderDto(
    [property: JsonRequired] Guid Id,
    [property: JsonRequired] List<OrderLineDto> Lines);

public sealed record OrderLineDto(
    [property: JsonRequired] string ProductName,
    [property: JsonRequired] int Quantity,
    [property: JsonRequired] decimal Price);