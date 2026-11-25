using Core.Events;

namespace Core.Interfaces;

public interface INotificationSink
{
    void Notify(OrderChangedEventArgs notification);
}