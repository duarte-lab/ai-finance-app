using Application.Accounts.Interfaces;
using Application.MonthlyClosing.DTOs;
using Application.MonthlyClosing.Interfaces;
using Application.MonthlyClosing.UseCases;
using Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace Application.Tests.MonthlyClosing;

public class ReopenMonthlyClosingUseCaseTests
{
    [Fact]
    public async Task ReopenMonthlyClosing_WhenClosingExists_ShouldUnpayClosingAccountsAndMarkReopened()
    {
        var accountOneId = Guid.NewGuid();
        var accountTwoId = Guid.NewGuid();

        var closing = new Domain.Entities.MonthlyClosing
        {
            Id = Guid.NewGuid(),
            Year = 2026,
            Month = 5,
            ClosedAtUtc = new DateTime(2026, 5, 31, 0, 0, 0, DateTimeKind.Utc),
            ReopenedAtUtc = null,
            AccountIds = [accountOneId, accountTwoId],
            Participants = ["Ana", "Bruno"],
            TotalAmount = 1500m,
            AmountPerPerson = 750m,
        };

        var accountRepositoryMock = new Mock<IAccountRepository>();
        accountRepositoryMock
            .Setup(x => x.GetAllAsync(2026, 5))
            .ReturnsAsync(
            [
                new Account
                {
                    Id = accountOneId,
                    Name = "Rent",
                    Amount = 1000m,
                    DueDate = new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc),
                    Paid = true,
                },
                new Account
                {
                    Id = accountTwoId,
                    Name = "Internet",
                    Amount = 500m,
                    DueDate = new DateTime(2026, 5, 11, 0, 0, 0, DateTimeKind.Utc),
                    Paid = true,
                },
            ]);

        var closingRepositoryMock = new Mock<IMonthlyClosingRepository>();
        closingRepositoryMock
            .Setup(x => x.GetByYearMonthAsync(2026, 5))
            .ReturnsAsync(closing);

        var useCase = new ReopenMonthlyClosingUseCase(accountRepositoryMock.Object, closingRepositoryMock.Object);

        var result = await useCase.ExecuteAsync(new ReopenMonthlyClosingRequest(2026, 5));

        result.IsReopened.Should().BeTrue();
        result.ReopenedAtUtc.Should().NotBeNull();
        accountRepositoryMock.Verify(
            x => x.UpdateAsync(It.Is<Account>(a => (a.Id == accountOneId || a.Id == accountTwoId) && !a.Paid)),
            Times.Exactly(2));
        closingRepositoryMock.Verify(
            x => x.UpdateAsync(It.Is<Domain.Entities.MonthlyClosing>(c => c.ReopenedAtUtc != null)),
            Times.Once);
    }

    [Fact]
    public async Task ReopenMonthlyClosing_WhenNoActiveClosing_ShouldThrowError()
    {
        var accountRepositoryMock = new Mock<IAccountRepository>();

        var closingRepositoryMock = new Mock<IMonthlyClosingRepository>();
        closingRepositoryMock
            .Setup(x => x.GetByYearMonthAsync(2026, 5))
            .ReturnsAsync((Domain.Entities.MonthlyClosing?)null);

        var useCase = new ReopenMonthlyClosingUseCase(accountRepositoryMock.Object, closingRepositoryMock.Object);

        var action = async () => await useCase.ExecuteAsync(new ReopenMonthlyClosingRequest(2026, 5));

        await action.Should().ThrowAsync<InvalidOperationException>();
    }
}
