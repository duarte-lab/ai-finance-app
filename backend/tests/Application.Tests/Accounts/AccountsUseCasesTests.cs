using Application.Accounts.DTOs;
using Application.Accounts.Interfaces;
using Application.Accounts.UseCases;
using Application.Auth.Interfaces;
using Application.MonthlyClosing.Interfaces;
using Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace Application.Tests.Accounts;

public class AccountsUseCasesTests
{
    private static Mock<ICurrentUserContext> CreateCurrentUserMock()
    {
        var mock = new Mock<ICurrentUserContext>();
        mock.Setup(x => x.TenantId).Returns(Guid.NewGuid());
        mock.Setup(x => x.UserId).Returns(Guid.NewGuid());
        return mock;
    }

    [Fact]
    public async Task CreateAccount_ValidRequest_CreatesWithPaidFalseAndUtcDate()
    {
        var repositoryMock = new Mock<IAccountRepository>();
        var useCase = new CreateAccountUseCase(repositoryMock.Object, CreateCurrentUserMock().Object);
        var dueDate = new DateTime(2026, 5, 10, 10, 0, 0, DateTimeKind.Local);

        var result = await useCase.ExecuteAsync(new CreateAccountRequest("Internet", 120.50m, dueDate));

        result.Name.Should().Be("Internet");
        result.Amount.Should().Be(120.50m);
        result.Paid.Should().BeFalse();
        result.DueDate.Kind.Should().Be(DateTimeKind.Utc);
        result.CreatedAtUtc.Kind.Should().Be(DateTimeKind.Utc);
        result.ParticipatesInDivision.Should().BeFalse();
        repositoryMock.Verify(
            x => x.CreateAsync(It.Is<Account>(a =>
                a.Name == "Internet" &&
                a.Amount == 120.50m &&
                a.Paid == false &&
                a.DueDate.Kind == DateTimeKind.Utc)),
            Times.Once);
    }

    [Fact]
    public async Task CreateAccount_NegativeAmount_ThrowsArgumentException()
    {
        var repositoryMock = new Mock<IAccountRepository>();
        var useCase = new CreateAccountUseCase(repositoryMock.Object, CreateCurrentUserMock().Object);

        var action = async () => await useCase.ExecuteAsync(
            new CreateAccountRequest("Rent", -1m, DateTime.UtcNow));

        await action.Should().ThrowAsync<ArgumentException>();
        repositoryMock.Verify(x => x.CreateAsync(It.IsAny<Account>()), Times.Never);
    }

    [Fact]
    public async Task CreateAccount_RetroactiveDueDate_ShouldAllowCreation()
    {
        var repositoryMock = new Mock<IAccountRepository>();
        var useCase = new CreateAccountUseCase(repositoryMock.Object, CreateCurrentUserMock().Object);
        var pastDueDate = DateTime.UtcNow.AddDays(-30);

        var result = await useCase.ExecuteAsync(new CreateAccountRequest("Past bill", 50m, pastDueDate));

        result.Name.Should().Be("Past bill");
        result.DueDate.Should().Be(pastDueDate);
        result.Paid.Should().BeFalse();
        repositoryMock.Verify(x => x.CreateAsync(It.IsAny<Account>()), Times.Once);
    }

    [Fact]
    public async Task MarkAccountAsPaid_ExistingAccount_MarksAndUpdates()
    {
        var id = Guid.NewGuid();
        var account = new Account
        {
            Id = id,
            Name = "Energy",
            Amount = 300m,
            DueDate = DateTime.UtcNow,
            Paid = false
        };

        var repositoryMock = new Mock<IAccountRepository>();
        repositoryMock.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(account);

        var useCase = new MarkAccountAsPaidUseCase(repositoryMock.Object);

        var result = await useCase.ExecuteAsync(id);

        result.Should().NotBeNull();
        result!.Paid.Should().BeTrue();
        repositoryMock.Verify(x => x.UpdateAsync(It.Is<Account>(a => a.Id == id && a.Paid)), Times.Once);
    }

    [Fact]
    public async Task UpdateAccountDivisionParticipation_ExistingAccount_ShouldUpdateFlag()
    {
        var id = Guid.NewGuid();
        var account = new Account
        {
            Id = id,
            Name = "Internet",
            Amount = 100m,
            DueDate = DateTime.UtcNow,
            CreatedAtUtc = DateTime.UtcNow.AddDays(-10),
            Paid = false,
            ParticipatesInDivision = false,
        };

        var repositoryMock = new Mock<IAccountRepository>();
        repositoryMock.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(account);

        var useCase = new UpdateAccountDivisionParticipationUseCase(repositoryMock.Object);

        var result = await useCase.ExecuteAsync(id, true);

        result.Should().NotBeNull();
        result!.ParticipatesInDivision.Should().BeTrue();
        repositoryMock.Verify(
            x => x.UpdateAsync(It.Is<Account>(a => a.Id == id && a.ParticipatesInDivision)),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAccount_WhenMonthIsOpen_ShouldDelete()
    {
        var id = Guid.NewGuid();
        var account = new Account
        {
            Id = id,
            Name = "Rent",
            Amount = 1200m,
            DueDate = new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc),
        };

        var repositoryMock = new Mock<IAccountRepository>();
        repositoryMock.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(account);
        repositoryMock.Setup(x => x.DeleteAsync(id)).ReturnsAsync(true);

        var closingMock = new Mock<IMonthlyClosingRepository>();
        closingMock.Setup(x => x.GetActiveByYearMonthAsync(2026, 5)).ReturnsAsync((Domain.Entities.MonthlyClosing?)null);

        var useCase = new DeleteAccountUseCase(repositoryMock.Object, closingMock.Object);

        var result = await useCase.ExecuteAsync(id);

        result.Should().BeTrue();
        repositoryMock.Verify(x => x.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task DeleteAccount_WhenMonthIsClosed_ShouldThrowInvalidOperationException()
    {
        var id = Guid.NewGuid();
        var account = new Account
        {
            Id = id,
            Name = "Rent",
            Amount = 1200m,
            DueDate = new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc),
        };

        var activeClosing = new Domain.Entities.MonthlyClosing
        {
            Id = Guid.NewGuid(),
            Year = 2026,
            Month = 5,
            ClosedAtUtc = DateTime.UtcNow,
            AccountIds = [],
            Participants = [],
        };

        var repositoryMock = new Mock<IAccountRepository>();
        repositoryMock.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(account);

        var closingMock = new Mock<IMonthlyClosingRepository>();
        closingMock.Setup(x => x.GetActiveByYearMonthAsync(2026, 5)).ReturnsAsync(activeClosing);

        var useCase = new DeleteAccountUseCase(repositoryMock.Object, closingMock.Object);

        var action = async () => await useCase.ExecuteAsync(id);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*closed month*");
        repositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }
}
