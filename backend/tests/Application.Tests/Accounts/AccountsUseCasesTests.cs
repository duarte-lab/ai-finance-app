using Application.Accounts.DTOs;
using Application.Accounts.Interfaces;
using Application.Accounts.UseCases;
using Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace Application.Tests.Accounts;

public class AccountsUseCasesTests
{
    [Fact]
    public async Task CreateAccount_ValidRequest_CreatesWithPaidFalseAndUtcDate()
    {
        var repositoryMock = new Mock<IAccountRepository>();
        var useCase = new CreateAccountUseCase(repositoryMock.Object);
        var dueDate = new DateTime(2026, 5, 10, 10, 0, 0, DateTimeKind.Local);

        var result = await useCase.ExecuteAsync(new CreateAccountRequest("Internet", 120.50m, dueDate));

        result.Name.Should().Be("Internet");
        result.Amount.Should().Be(120.50m);
        result.Paid.Should().BeFalse();
        result.DueDate.Kind.Should().Be(DateTimeKind.Utc);
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
        var useCase = new CreateAccountUseCase(repositoryMock.Object);

        var action = async () => await useCase.ExecuteAsync(
            new CreateAccountRequest("Rent", -1m, DateTime.UtcNow));

        await action.Should().ThrowAsync<ArgumentException>();
        repositoryMock.Verify(x => x.CreateAsync(It.IsAny<Account>()), Times.Never);
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
}
