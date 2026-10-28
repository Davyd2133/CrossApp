using System.Globalization;
using System.Text;
using Core.Dto;

namespace Core.Import;

public static class MixedCsvImporter
{
    private const char Separator = ';';

    private const NumberStyles PriceStyles =
        NumberStyles.AllowLeadingSign |
        NumberStyles.AllowDecimalPoint;

    public static ImportResult<object> Load(string path)
    {
        var items = new List<object>();
        var errors = new List<string>();

        string[] lines = File.ReadAllLines(
            path,
            Encoding.UTF8);

        for (int i = 0; i < lines.Length; i++)
        {
            int number = i + 1;

            string line = lines[i].Trim();

            if (string.IsNullOrWhiteSpace(line) ||
                line.StartsWith('#'))
            {
                continue;
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

        return new ImportResult<object>(
            items.AsReadOnly(),
            errors.AsReadOnly());
    }

    private static ParseOutcome ParseLine(string line)
    {
        string[] parts = line.Split(
            Separator,
            StringSplitOptions.TrimEntries);

        return parts switch
        {
            ["P", "", _, _] or ["P", _, "", _] =>
                new ParseFailed(
                    "ідентифікатор або назва товару порожні"),

            ["P", _, _, var priceText]
                when !decimal.TryParse(
                    priceText,
                    PriceStyles,
                    CultureInfo.InvariantCulture,
                    out decimal price) || price < 0 =>
                new ParseFailed(
                    $"ціна '{priceText}' не є невід'ємним числом"),

            ["P", var id, var name, var priceText] =>
                new ParseOk(
                    new ProductDto(
                        id,
                        name,
                        decimal.Parse(
                            priceText,
                            PriceStyles,
                            CultureInfo.InvariantCulture))),

            ["W", "", _] or ["W", _, ""] =>
                new ParseFailed(
                    "ідентифікатор або назва складу порожні"),

            ["W", var id, var name] =>
                new ParseOk(
                    new WarehouseDto(id, name)),

            ["P", ..] =>
                new ParseFailed(
                    "для товару очікую 4 колонки"),

            ["W", ..] =>
                new ParseFailed(
                    "для складу очікую 3 колонки"),

            [var prefix, ..] =>
                new ParseFailed(
                    $"невідомий префікс '{prefix}'"),

            _ =>
                new ParseFailed(
                    "невідомий формат рядка")
        };
    }

    private abstract record ParseOutcome;

    private sealed record ParseOk(
        object Value) : ParseOutcome;

    private sealed record ParseFailed(
        string Reason) : ParseOutcome;
}