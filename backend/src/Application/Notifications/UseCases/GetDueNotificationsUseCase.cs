using Application.Accounts.Interfaces;
using Application.Notifications.DTOs;
using Application.Notifications.Interfaces;

namespace Application.Notifications.UseCases;

public class GetDueNotificationsUseCase
{
    private readonly IAccountRepository _accountRepository;
    private readonly INotificationClock _clock;

    public GetDueNotificationsUseCase(
        IAccountRepository accountRepository,
        INotificationClock clock)
    {
        _accountRepository = accountRepository;
        _clock = clock;
    }

    public async Task<IReadOnlyCollection<NotificationResponse>> ExecuteAsync()
    {
        var todayUtc = _clock.UtcNow.Date;
        var accounts = await _accountRepository.GetAllAsync();

        var notifications = accounts
            .Where(account => !account.Paid)
            .Select(account =>
            {
                var dueDate = account.DueDate.Date;
                var daysUntilDue = (dueDate - todayUtc).Days;

                if (daysUntilDue == 0)
                {
                    return new NotificationResponse(
                        AccountId: account.Id,
                        AccountName: account.Name,
                        DueDateUtc: account.DueDate,
                        DaysUntilDue: daysUntilDue,
                        Type: NotificationType.DueToday,
                        Message: $"A conta '{account.Name}' vence hoje.");
                }

                if (daysUntilDue == 3)
                {
                    return new NotificationResponse(
                        AccountId: account.Id,
                        AccountName: account.Name,
                        DueDateUtc: account.DueDate,
                        DaysUntilDue: daysUntilDue,
                        Type: NotificationType.DueInThreeDays,
                        Message: $"A conta '{account.Name}' vence em 3 dias.");
                }

                return null;
            })
            .Where(item => item is not null)
            .Select(item => item!)
            .OrderBy(item => item.DueDateUtc)
            .ThenBy(item => item.AccountName)
            .ToList();

        return notifications;
    }
}