using System;
using Core.Events;
using Core.Interfaces;

namespace Cli.Notifications;

public sealed class ConsoleSink : INotificationSink
{
    public void Notify(OrderChangedEventArgs notification)
    {
        Console.WriteLine(
            $"[Sink: консоль] " +
            $"{notification.At:HH:mm:ss} " +
            $"{notification.Kind}; " +
            $"OrderId={notification.OrderId}; " +
            $"позицій={notification.LinesCount}; " +
            $"нова сума={notification.Total:F2} грн");
    }
}