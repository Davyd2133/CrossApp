/*using Core;

EnvironmentReport report = EnvironmentInfo.Collect();

Console.WriteLine("CrossApp — інформація про середовище");
Console.WriteLine(new string('-', 52));
Console.WriteLine($"ОС                 : {report.OsDescription}");
Console.WriteLine($"Runtime            : {report.FrameworkDescription}");
Console.WriteLine($"Архітектура        : {report.ProcessArchitecture}");
Console.WriteLine($"RID (визначено)    : {report.DetectedRid}");
Console.WriteLine($"RID (від .NET)     : {report.ReportedRid}");
Console.WriteLine($"Каталог            : {report.BaseDirectory}");
Console.WriteLine($"Цільова збірка Core: {report.BuildNote}");
*/

using System.Globalization;
using System.Text;
using Core.Dto;
using Core.Import;

Console.OutputEncoding = Encoding.UTF8;

string path = args.Length > 0
    ? args[0]
    : Path.Combine("data", "sample.csv");

try
{
    string fullPath = Path.GetFullPath(path);

    if (!File.Exists(fullPath))
    {
        Console.WriteLine(
            $"Файл не знайдено: {fullPath}");

        return 1;
    }

    ImportResult<ProductDto> result =
        ProductCsvImporter.Load(fullPath);

    Console.WriteLine(
        $"Завантажено записів: {result.Items.Count}");

    Console.WriteLine("Перші записи:");

    foreach (ProductDto product in result.Items.Take(5))
    {
        string price = product.Price.ToString(
            "F2",
            CultureInfo.InvariantCulture);

        Console.WriteLine(
            $"{product.Id,-6} " +
            $"{product.Name,-22} " +
            $"{price,10} грн");
    }

    Console.WriteLine(
        $"Пропущено рядків: {result.Errors.Count}");

    foreach (string error in result.Errors)
    {
        Console.WriteLine($" ! {error}");
    }

    return 0;
}
catch (UnauthorizedAccessException)
{
    Console.WriteLine(
        "Немає дозволу на читання файлу.");

    return 1;
}
catch (IOException exception)
{
    Console.WriteLine(
        $"Не вдалося прочитати файл: {exception.Message}");

    return 1;
}
catch (ArgumentException)
{
    Console.WriteLine(
        "Шлях до файлу має неправильний формат.");

    return 1;
}
catch (NotSupportedException)
{
    Console.WriteLine(
        "Формат шляху до файлу не підтримується.");

    return 1;
}