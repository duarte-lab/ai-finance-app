namespace Application.Notifications.Interfaces;

public interface INotificationClock
{
    DateTime UtcNow { get; }
}