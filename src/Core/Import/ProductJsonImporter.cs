using System.Text;
using System.Text.Json;
using Core.Dto;

namespace Core.Import;

public static class ProductJsonImporter
{
    public static ImportResult<ProductDto> Load(string path)
    {
        var items = new List<ProductDto>();
        var errors = new List<string>();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        string json = File.ReadAllText(path, Encoding.UTF8);

        using JsonDocument document = JsonDocument.Parse(json);

        if (document.RootElement.ValueKind != JsonValueKind.Array)
        {
            throw new JsonException(
                "Кореневим елементом JSON має бути масив.");
        }

        int number = 0;

        foreach (JsonElement element
            in document.RootElement.EnumerateArray())
        {
            number++;

            try
            {
                if (element.ValueKind != JsonValueKind.Object)
                {
                    errors.Add(
                        $"запис {number}: очікую JSON-об'єкт");

                    continue;
                }

                bool hasPrice = element
                    .EnumerateObject()
                    .Any(property =>
                        string.Equals(
                            property.Name,
                            "price",
                            StringComparison.OrdinalIgnoreCase));

                if (!hasPrice)
                {
                    errors.Add(
                        $"запис {number}: відсутнє поле price");

                    continue;
                }

                ProductDto? product =
                    element.Deserialize<ProductDto>(options);

                switch (product)
                {
                    case null:
                        errors.Add(
                            $"запис {number}: порожній об'єкт");
                        break;

                    case var p when
                        string.IsNullOrWhiteSpace(p.Id) ||
                        string.IsNullOrWhiteSpace(p.Name):
                        errors.Add(
                            $"запис {number}: " +
                            "ідентифікатор або назва порожні");
                        break;

                    case { Price: < 0 }:
                        errors.Add(
                            $"запис {number}: ціна від'ємна");
                        break;

                    default:
                        items.Add(
                            product with
                            {
                                Id = product.Id.Trim(),
                                Name = product.Name.Trim()
                            });
                        break;
                }
            }
            catch (JsonException)
            {
                errors.Add(
                    $"запис {number}: неправильний тип даних");
            }
        }

        return new ImportResult<ProductDto>(
            items.AsReadOnly(),
            errors.AsReadOnly());
    }
}