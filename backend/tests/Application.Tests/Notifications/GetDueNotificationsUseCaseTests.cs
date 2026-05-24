using Application.Accounts.Interfaces;
using Application.Notifications.DTOs;
using Application.Notifications.Interfaces;
using Application.Notifications.UseCases;
using Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace Application.Tests.Notifications;

public class GetDueNotificationsUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldReturnOnlyDueTodayAndDueInThreeDaysForUnpaidAccounts()
    {
        var baseDate = new DateTime(2026, 5, 23, 10, 0, 0, DateTimeKind.Utc);
        var repositoryMock = new Mock<IAccountRepository>();
        repositoryMock
            .Setup(x => x.GetAllAsync(null, null))
            .ReturnsAsync(
            [
                new Account
                {
                    Id = Guid.NewGuid(),
                    Name = "Rent",
                    Amount = 1200m,
                    DueDate = baseDate,
                    Paid = false,
                },
                new Account
                {
                    Id = Guid.NewGuid(),
                    Name = "Internet",
                    Amount = 200m,
                    DueDate = baseDate.AddDays(3),
                    Paid = false,
                },
                new Account
                {
                    Id = Guid.NewGuid(),
                    Name = "Gym",
                    Amount = 99m,
                    DueDate = baseDate.AddDays(2),
                    Paid = false,
                },
                new Account
                {
                    Id = Guid.NewGuid(),
                    Name = "Energy",
                    Amount = 190m,
                    DueDate = baseDate,
                    Paid = true,
                },
            ]);

        var useCase = new GetDueNotificationsUseCase(
            repositoryMock.Object,
            new FixedNotificationClock(baseDate));

        var result = await useCase.ExecuteAsync();

        result.Should().HaveCount(2);
        result.Should().Contain(x => x.AccountName == "Rent" && x.Type == NotificationType.DueToday);
        result.Should().Contain(x => x.AccountName == "Internet" && x.Type == NotificationType.DueInThreeDays);
    }

    private sealed class FixedNotificationClock : INotificationClock
    {
        public FixedNotificationClock(DateTime utcNow)
        {
            UtcNow = utcNow;
        }

        public DateTime UtcNow { get; }
    }
}