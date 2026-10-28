using System;
using System.Globalization;
using System.IO;
using System.Text;
using Cli.Stores;
using Core.Events;
using Core.Interfaces;
using Core.Services;

Console.OutputEncoding = Encoding.UTF8;

var culture = CultureInfo.GetCultureInfo("uk-UA");

CultureInfo.CurrentCulture = culture;
CultureInfo.CurrentUICulture = culture;

// 1. Вибір сховища.

bool useJson = Array.Exists(
    args,
    argument => string.Equals(
        argument,
        "--json",
        StringComparison.OrdinalIgnoreCase));

IOrderStore store;

if (useJson)
{
    string dataPath = Path.Combine(
        AppContext.BaseDirectory,
        "data",
        "orders.json");

    store = new JsonOrderStore(dataPath);

    Console.WriteLine("Сховище: JSON");
}
else
{
    store = new InMemoryOrderStore();

    Console.WriteLine("Сховище: у пам'яті");
}

var service = new OrderService(store);

// 2. Підготовка журналу.

string logPath = Path.Combine(
    AppContext.BaseDirectory,
    "logs",
    "app.log");

Directory.CreateDirectory(
    Path.GetDirectoryName(logPath)!);

Console.WriteLine($"Файл журналу: {logPath}");

// 3. Перший підписник: консоль і файл.

void OnOrderChanged(
    object? sender,
    OrderChangedEventArgs e)
{
    string line =
        $"{e.At:HH:mm:ss} {e.Kind,-14} " +
        $"OrderId={e.OrderId}; " +
        $"позицій={e.LinesCount}; " +
        $"нова сума={e.Total:F2} грн";

    Console.WriteLine($"[1: журнал] {line}");

    try
    {
        File.AppendAllText(
            logPath,
            line + Environment.NewLine);
    }
    catch (IOException ex)
    {
        Console.Error.WriteLine(
            $"Не вдалося записати журнал: {ex.Message}");
    }
    catch (UnauthorizedAccessException ex)
    {
        Console.Error.WriteLine(
            $"Немає доступу до журналу: {ex.Message}");
    }
}

// 4. Другий підписник: лямбда із замиканням.

int changes = 0;

EventHandler<OrderChangedEventArgs> counterHandler = (_, e) =>
{
    changes++;

    Console.WriteLine(
        $"[2: лічильник] {e.Kind}; подій отримано: {changes}");
};

// Порядок підписки визначає порядок виклику.

service.Changed += OnOrderChanged;
service.Changed += counterHandler;

// 5. Успішні операції.

Console.WriteLine("\n--- Створення замовлення ---");

var created = service.Create();

Console.WriteLine("\n--- Додавання першої позиції ---");

service.AddLine(
    created.Id,
    "Клавіатура",
    800m,
    1);

Console.WriteLine("\n--- Додавання другої позиції ---");

service.AddLine(
    created.Id,
    "Миша",
    400m,
    2);

Console.WriteLine($"\nПодій отримано: {changes}");

// 6. Помилка інваріанта: від'ємна кількість.

Console.WriteLine("\n--- Перевірка неправильної кількості ---");

int beforeFailure = changes;

try
{
    service.AddLine(
        created.Id,
        "Монітор",
        5000m,
        -1);
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Помилка: {ex.Message}");
}

Console.WriteLine(
    $"Лічильник до/після: {beforeFailure}/{changes}");

var afterFailure = service.Find(created.Id)!;

Console.WriteLine(
    $"Після відмови: позицій={afterFailure.LinesCount}; " +
    $"сума={afterFailure.Total:F2} грн");

// 7. Помилка пошуку: неіснуючий id.

Console.WriteLine("\n--- Перевірка неіснуючого id ---");

beforeFailure = changes;

try
{
    service.Confirm("missing-order");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Помилка: {ex.Message}");
}

Console.WriteLine(
    $"Лічильник до/після: {beforeFailure}/{changes}");

// 8. Ще одна успішна операція.

Console.WriteLine("\n--- Додавання третьої позиції ---");

service.AddLine(
    created.Id,
    "Килимок",
    200m,
    1);

Console.WriteLine($"\nПодій отримано: {changes}");

// 9. Відписка першого підписника.

service.Changed -= OnOrderChanged;

Console.WriteLine("\n--- Журнал відписано. Підтвердження ---");

service.Confirm(created.Id);

// Тут працює тільки counterHandler.
// Рядка [1: журнал] немає.
// Запису OrderConfirmed у файлі журналу цього запуску немає.

var confirmed = service.Find(created.Id)!;

Console.WriteLine(
    $"Підтверджено: {confirmed.IsConfirmed}; " +
    $"позицій={confirmed.LinesCount}; " +
    $"сума={confirmed.Total:F2} грн");

Console.WriteLine($"Подій отримано: {changes}");

// 10. Пошук із Func<Order, bool>.

Console.WriteLine(
    "\n--- Підтверджені замовлення із сумою від 1500 грн ---");

var found = service.FindAll(
    order => order.IsConfirmed && order.Total >= 1500m);

foreach (var order in found)
{
    Console.WriteLine(
        $"Id={order.Id}; " +
        $"позицій={order.LinesCount}; " +
        $"сума={order.Total:F2} грн");
}

// 11. Відписка другого підписника.

service.Changed -= counterHandler;

Console.WriteLine("\n--- Обох підписників відписано ---");

var silent = service.Create();

service.AddLine(
    silent.Id,
    "Блокнот",
    100m,
    1);

var silentResult = service.Find(silent.Id)!;

Console.WriteLine(
    $"Без підписників створено замовлення: " +
    $"позицій={silentResult.LinesCount}; " +
    $"сума={silentResult.Total:F2} грн");

Console.WriteLine(
    $"Лічильник залишився: {changes}");