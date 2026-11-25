using System;
using System.IO;
using Core.Events;
using Core.Interfaces;

namespace Cli.Notifications;

public sealed class FileSink : INotificationSink
{
    private readonly string _path;

    public FileSink(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException(
                "Шлях до журналу не може бути порожнім.",
                nameof(path));
        }

        _path = Path.GetFullPath(path);

        Directory.CreateDirectory(
            Path.GetDirectoryName(_path)!);
    }

    public void Notify(OrderChangedEventArgs notification)
    {
        string line =
            $"{notification.At:HH:mm:ss} " +
            $"{notification.Kind}; " +
            $"OrderId={notification.OrderId}; " +
            $"позицій={notification.LinesCount}; " +
            $"нова сума={notification.Total:F2} грн";

        try
        {
            File.AppendAllText(
                _path,
                line + Environment.NewLine);
        }
        catch (IOException ex)
        {
            Console.Error.WriteLine(
                $"Помилка запису журналу: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.Error.WriteLine(
                $"Немає доступу до журналу: {ex.Message}");
        }
    }
}