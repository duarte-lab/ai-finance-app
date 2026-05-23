using Application.Accounts.Interfaces;
using Application.MonthlyClosing.DTOs;
using Application.MonthlyClosing.Interfaces;
using Application.MonthlyClosing.UseCases;
using Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace Application.Tests.MonthlyClosing;

public class CreateMonthlyClosingUseCaseTests
{
    [Fact]
    public async Task CreateMonthlyClosing_WithMultipleAccounts_ShouldCalculateEqualDivision()
    {
        var accountOneId = Guid.NewGuid();
        var accountTwoId = Guid.NewGuid();

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
                    Paid = false,
                },
                new Account
                {
                    Id = accountTwoId,
                    Name = "Internet",
                    Amount = 500m,
                    DueDate = new DateTime(2026, 5, 15, 0, 0, 0, DateTimeKind.Utc),
                    Paid = false,
                },
                new Account
                {
                    Id = Guid.NewGuid(),
                    Name = "Energy",
                    Amount = 120m,
                    DueDate = new DateTime(2026, 5, 20, 0, 0, 0, DateTimeKind.Utc),
                    Paid = true,
                },
            ]);

        var closingRepositoryMock = new Mock<IMonthlyClosingRepository>();

        var useCase = new CreateMonthlyClosingUseCase(accountRepositoryMock.Object, closingRepositoryMock.Object);

        var result = await useCase.ExecuteAsync(
            new CreateMonthlyClosingRequest(
                Year: 2026,
                Month: 5,
                AccountIds: [accountOneId, accountTwoId],
                Participants: ["Ana", "Bruno", "Carla"]));

        result.Year.Should().Be(2026);
        result.Month.Should().Be(5);
        result.TotalAmount.Should().Be(1500m);
        result.AmountPerPerson.Should().Be(500m);
        result.AccountCount.Should().Be(2);
        result.ParticipantCount.Should().Be(3);
        result.ClosedAtUtc.Kind.Should().Be(DateTimeKind.Utc);

        closingRepositoryMock.Verify(
            x => x.CreateAsync(It.Is<Domain.Entities.MonthlyClosing>(closing =>
                closing.Year == 2026 &&
                closing.Month == 5 &&
                closing.TotalAmount == 1500m &&
                closing.AmountPerPerson == 500m &&
                closing.AccountIds.Count == 2 &&
                closing.Participants.Count == 3)),
            Times.Once);
    }

    [Fact]
    public async Task CreateMonthlyClosing_WithoutSelectedAccounts_ShouldThrowError()
    {
        var accountRepositoryMock = new Mock<IAccountRepository>();
        var closingRepositoryMock = new Mock<IMonthlyClosingRepository>();

        var useCase = new CreateMonthlyClosingUseCase(accountRepositoryMock.Object, closingRepositoryMock.Object);

        var action = async () => await useCase.ExecuteAsync(
            new CreateMonthlyClosingRequest(
                Year: 2026,
                Month: 5,
                AccountIds: [],
                Participants: ["Ana", "Bruno"]));

        await action.Should().ThrowAsync<InvalidOperationException>();
        closingRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Domain.Entities.MonthlyClosing>()), Times.Never);
    }
}
