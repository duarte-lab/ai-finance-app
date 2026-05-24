using Application.Notifications.DTOs;
using Application.Notifications.UseCases;

namespace Api.Notifications;

public class NotificationsBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NotificationsBackgroundService> _logger;

    public NotificationsBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<NotificationsBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var useCase = scope.ServiceProvider.GetRequiredService<GetDueNotificationsUseCase>();
                var notifications = await useCase.ExecuteAsync();

                if (notifications.Count > 0)
                {
                    var dueToday = notifications.Count(x => x.Type == NotificationType.DueToday);
                    var dueInThreeDays = notifications.Count(x => x.Type == NotificationType.DueInThreeDays);

                    _logger.LogInformation(
                        "Notifications check: {Total} alert(s). Due today: {DueToday}. Due in 3 days: {DueInThreeDays}.",
                        notifications.Count,
                        dueToday,
                        dueInThreeDays);
                }
                else
                {
                    _logger.LogInformation("Notifications check: no due alerts.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Notifications background job failed.");
            }

            await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
        }
    }
}