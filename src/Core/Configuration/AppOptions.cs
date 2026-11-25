using System;
using System.IO;

namespace CrossApp.Lab09.Configuration;

public sealed class AppOptions
{
    public string EnvironmentName { get; set; } = "Production";
    public StorageOptions Storage { get; set; } = new();
    public LoggingOptions Logging { get; set; } = new();
    public OrdersOptions Orders { get; set; } = new();
}

public sealed class StorageOptions
{
    public string DataPath { get; set; } = "data";
    public string FileName { get; set; } = "orders.json";

    public string ResolveFullPath(string baseDirectory)
    {
        if (string.IsNullOrWhiteSpace(DataPath))
            throw new ArgumentException("Storage:DataPath має містити шлях до каталогу.");

        if (string.IsNullOrWhiteSpace(FileName) ||
            FileName != Path.GetFileName(FileName) ||
            FileName is "." or "..")
        {
            throw new ArgumentException("Storage:FileName має містити назву файлу.");
        }

        var directory = Path.GetFullPath(DataPath, baseDirectory);
        return Path.Combine(directory, FileName);
    }
}

public sealed class LoggingOptions
{
    public string MinimumLevel { get; set; } = "Information";
}

public sealed class OrdersOptions
{
    public string Currency { get; set; } = "UAH";
}