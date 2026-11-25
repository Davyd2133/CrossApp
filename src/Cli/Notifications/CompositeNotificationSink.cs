using System;
using Core.Events;
using Core.Interfaces;

namespace Cli.Notifications;

public sealed class CompositeNotificationSink : INotificationSink
{
    private readonly INotificationSink[] _sinks;

    public CompositeNotificationSink(
        params INotificationSink[] sinks)
    {
        ArgumentNullException.ThrowIfNull(sinks);

        if (Array.Exists(sinks, sink => sink is null))
        {
            throw new ArgumentException(
                "Список не може містити null.",
                nameof(sinks));
        }

        _sinks = (INotificationSink[])sinks.Clone();
    }

    public void Notify(OrderChangedEventArgs notification)
    {
        foreach (var sink in _sinks)
        {
            sink.Notify(notification);
        }
    }
}