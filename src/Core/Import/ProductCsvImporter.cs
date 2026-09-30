using System.Globalization;
using System.Text;
using Core.Dto;

namespace Core.Import;

public static class ProductCsvImporter
{
    private const char Separator = ';';

    private const NumberStyles PriceStyles =
        NumberStyles.AllowLeadingSign |
        NumberStyles.AllowDecimalPoint;

    public static ImportResult<ProductDto> Load(string path)
    {
        var items = new List<ProductDto>();
        var errors = new List<string>();

        string[] lines = File.ReadAllLines(
            path,
            Encoding.UTF8);

        bool firstContentLine = true;

        for (int i = 0; i < lines.Length; i++)
        {
            int number = i + 1;

            string line = lines[i].Trim();

            if (string.IsNullOrWhiteSpace(line) ||
                line.StartsWith('#'))
            {
                continue;
            }

            if (firstContentLine)
            {
                firstContentLine = false;

                if (IsHeader(line))
                {
                    continue;
                }
            }

            switch (ParseLine(line))
            {
                case ParseOk ok:
                    items.Add(ok.Value);
                    break;

                case ParseFailed failed:
                    errors.Add(
                        $"рядок {number}: {failed.Reason}");
                    break;
            }
        }

        return new ImportResult<ProductDto>(
            items.AsReadOnly(),
            errors.AsReadOnly());
    }

    private static bool IsHeader(string line)
    {
        string[] parts = line.Split(
            Separator,
            StringSplitOptions.TrimEntries);

        return parts.Length == 3 &&
            string.Equals(
                parts[0],
                "id",
                StringComparison.OrdinalIgnoreCase) &&
            string.Equals(
                parts[1],
                "name",
                StringComparison.OrdinalIgnoreCase) &&
            string.Equals(
                parts[2],
                "price",
                StringComparison.OrdinalIgnoreCase);
    }

    private static ParseOutcome ParseLine(string line)
    {
        string[] parts = line.Split(
            Separator,
            StringSplitOptions.TrimEntries);

        return parts switch
        {
            { Length: < 3 } =>
                new ParseFailed(
                    $"очікую 3 колонки, отримав {parts.Length}"),

            { Length: > 3 } =>
                new ParseFailed(
                    $"занадто багато колонок: {parts.Length}"),

            ["", _, _] or [_, "", _] =>
                new ParseFailed(
                    "ідентифікатор або назва порожні"),

            [_, _, var priceText]
                when !decimal.TryParse(
                    priceText,
                    PriceStyles,
                    CultureInfo.InvariantCulture,
                    out decimal price) || price < 0 =>
                new ParseFailed(
                    $"ціна '{priceText}' не є невід'ємним числом"),

            [var id, var name, var priceText] =>
                new ParseOk(
                    new ProductDto(
                        id,
                        name,
                        decimal.Parse(
                            priceText,
                            PriceStyles,
                            CultureInfo.InvariantCulture))),

            _ =>
                new ParseFailed(
                    "невідомий формат рядка")
        };
    }

    private abstract record ParseOutcome;

    private sealed record ParseOk(
        ProductDto Value) : ParseOutcome;

    private sealed record ParseFailed(
        string Reason) : ParseOutcome;
}