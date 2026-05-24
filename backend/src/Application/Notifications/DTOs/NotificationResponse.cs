namespace Application.Notifications.DTOs;

public enum NotificationType
{
    DueInThreeDays,
    DueToday,
}

public record NotificationResponse(
    Guid AccountId,
    string AccountName,
    DateTime DueDateUtc,
    int DaysUntilDue,
    NotificationType Type,
    string Message);