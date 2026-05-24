using Application.Notifications.Interfaces;

namespace Api.Notifications;

public class SystemNotificationClock : INotificationClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}