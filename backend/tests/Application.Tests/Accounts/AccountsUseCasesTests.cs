using Application.Accounts.DTOs;
using Application.Accounts.Interfaces;
using Application.Accounts.UseCases;
using Application.People.Interfaces;
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
        var personRepositoryMock = new Mock<IPersonRepository>();
        var useCase = new CreateAccountUseCase(repositoryMock.Object, personRepositoryMock.Object);
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
        var personRepositoryMock = new Mock<IPersonRepository>();
        var useCase = new CreateAccountUseCase(repositoryMock.Object, personRepositoryMock.Object);

        var action = async () => await useCase.ExecuteAsync(
            new CreateAccountRequest("Rent", -1m, DateTime.UtcNow));

        await action.Should().ThrowAsync<ArgumentException>();
        repositoryMock.Verify(x => x.CreateAsync(It.IsAny<Account>()), Times.Never);
    }

    [Fact]
    public async Task CreateAccount_WithDuplicateParticipant_ShouldThrowArgumentException()
    {
        var personId = Guid.NewGuid();
        var repositoryMock = new Mock<IAccountRepository>();
        var personRepositoryMock = new Mock<IPersonRepository>();
        var useCase = new CreateAccountUseCase(repositoryMock.Object, personRepositoryMock.Object);

        var action = async () => await useCase.ExecuteAsync(
            new CreateAccountRequest(
                "Rent",
                1500m,
                DateTime.UtcNow,
                [
                    new AccountParticipantRequest(personId, 50m),
                    new AccountParticipantRequest(personId, 50m),
                ]));

        await action.Should().ThrowAsync<ArgumentException>();
        repositoryMock.Verify(x => x.CreateAsync(It.IsAny<Account>()), Times.Never);
    }

    [Fact]
    public async Task CreateAccount_WithParticipantsSummingNot100_ShouldThrowArgumentException()
    {
        var repositoryMock = new Mock<IAccountRepository>();
        var personRepositoryMock = new Mock<IPersonRepository>();
        var useCase = new CreateAccountUseCase(repositoryMock.Object, personRepositoryMock.Object);

        var action = async () => await useCase.ExecuteAsync(
            new CreateAccountRequest(
                "Rent",
                1500m,
                DateTime.UtcNow,
                [
                    new AccountParticipantRequest(Guid.NewGuid(), 40m),
                    new AccountParticipantRequest(Guid.NewGuid(), 40m),
                ]));

        await action.Should().ThrowAsync<ArgumentException>();
        repositoryMock.Verify(x => x.CreateAsync(It.IsAny<Account>()), Times.Never);
    }

    [Fact]
    public async Task CreateAccount_WithValidParticipants_ShouldPersistParticipants()
    {
        var personOneId = Guid.NewGuid();
        var personTwoId = Guid.NewGuid();
        var repositoryMock = new Mock<IAccountRepository>();
        var personRepositoryMock = new Mock<IPersonRepository>();
        personRepositoryMock
            .Setup(x => x.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>()))
            .ReturnsAsync(
            [
                new Person { Id = personOneId, Name = "Ana" },
                new Person { Id = personTwoId, Name = "Bruno" },
            ]);

        var useCase = new CreateAccountUseCase(repositoryMock.Object, personRepositoryMock.Object);

        var result = await useCase.ExecuteAsync(
            new CreateAccountRequest(
                "Rent",
                1500m,
                DateTime.UtcNow,
                [
                    new AccountParticipantRequest(personOneId, 60m),
                    new AccountParticipantRequest(personTwoId, 40m),
                ]));

        result.Participants.Should().HaveCount(2);
        result.Participants.Sum(x => x.Percentage).Should().Be(100m);

        repositoryMock.Verify(
            x => x.CreateAsync(It.Is<Account>(a =>
                a.Participants.Count == 2 &&
                a.Participants.Sum(p => p.Percentage) == 100m)),
            Times.Once);
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
