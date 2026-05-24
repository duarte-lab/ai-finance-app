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
        var personRepositoryMock = new Mock<IPersonRepository>();
        var useCase = new CreateAccountUseCase(repositoryMock.Object, personRepositoryMock.Object);

        var action = async () => await useCase.ExecuteAsync(
            new CreateAccountRequest("Rent", -1m, DateTime.UtcNow));

        await action.Should().ThrowAsync<ArgumentException>();
        repositoryMock.Verify(x => x.CreateAsync(It.IsAny<Account>()), Times.Never);
    }

    [Fact]
    public async Task CreateAccount_RetroactiveDueDate_ShouldAllowCreation()
    {
        var repositoryMock = new Mock<IAccountRepository>();
        var personRepositoryMock = new Mock<IPersonRepository>();
        var useCase = new CreateAccountUseCase(repositoryMock.Object, personRepositoryMock.Object);
        var pastDueDate = DateTime.UtcNow.AddDays(-30);

        var result = await useCase.ExecuteAsync(new CreateAccountRequest("Past bill", 50m, pastDueDate));

        result.Name.Should().Be("Past bill");
        result.DueDate.Should().Be(pastDueDate);
        result.Paid.Should().BeFalse();
        repositoryMock.Verify(x => x.CreateAsync(It.IsAny<Account>()), Times.Once);
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
                    new AccountParticipantRequest(personId),
                    new AccountParticipantRequest(personId),
                ]));

        await action.Should().ThrowAsync<ArgumentException>();
        repositoryMock.Verify(x => x.CreateAsync(It.IsAny<Account>()), Times.Never);
    }

    [Fact]
    public async Task CreateAccount_WithUnknownParticipant_ShouldThrowArgumentException()
    {
        var repositoryMock = new Mock<IAccountRepository>();
        var personRepositoryMock = new Mock<IPersonRepository>();
        personRepositoryMock
            .Setup(x => x.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>()))
            .ReturnsAsync(Array.Empty<Person>());
        var useCase = new CreateAccountUseCase(repositoryMock.Object, personRepositoryMock.Object);

        var action = async () => await useCase.ExecuteAsync(
            new CreateAccountRequest(
                "Rent",
                1500m,
                DateTime.UtcNow,
                [
                    new AccountParticipantRequest(Guid.NewGuid()),
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
                    new AccountParticipantRequest(personOneId),
                    new AccountParticipantRequest(personTwoId),
                ]));

        result.Participants.Should().HaveCount(2);
        result.Participants.Select(x => x.PersonId).Should().BeEquivalentTo([personOneId, personTwoId]);

        repositoryMock.Verify(
            x => x.CreateAsync(It.Is<Account>(a =>
                a.Participants.Count == 2)),
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
}
