using Application.People.DTOs;
using Application.People.Interfaces;
using Application.People.UseCases;
using Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace Application.Tests.People;

public class PeopleUseCasesTests
{
    [Fact]
    public async Task CreatePerson_ValidRequest_ShouldCreateWithUtcDate()
    {
        var repositoryMock = new Mock<IPersonRepository>();
        var useCase = new CreatePersonUseCase(repositoryMock.Object);

        var result = await useCase.ExecuteAsync(new CreatePersonRequest("Ana"));

        result.Name.Should().Be("Ana");
        result.Id.Should().NotBeEmpty();
        result.CreatedAtUtc.Kind.Should().Be(DateTimeKind.Utc);

        repositoryMock.Verify(
            x => x.CreateAsync(It.Is<Person>(p =>
                p.Name == "Ana" &&
                p.CreatedAtUtc.Kind == DateTimeKind.Utc)),
            Times.Once);
    }

    [Fact]
    public async Task CreatePerson_NameLongerThan50_ShouldThrowArgumentException()
    {
        var repositoryMock = new Mock<IPersonRepository>();
        var useCase = new CreatePersonUseCase(repositoryMock.Object);
        var invalidName = new string('A', 51);

        Func<Task> act = async () => await useCase.ExecuteAsync(new CreatePersonRequest(invalidName));

        await act.Should().ThrowAsync<ArgumentException>();
        repositoryMock.Verify(x => x.CreateAsync(It.IsAny<Person>()), Times.Never);
    }

    [Fact]
    public async Task GetPeople_ShouldReturnAlphabeticalOrder()
    {
        var repositoryMock = new Mock<IPersonRepository>();
        repositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(
            [
                new Person { Id = Guid.NewGuid(), Name = "Bruno", CreatedAtUtc = DateTime.UtcNow, DeletedAtUtc = null },
                new Person { Id = Guid.NewGuid(), Name = "Ana", CreatedAtUtc = DateTime.UtcNow, DeletedAtUtc = null },
            ]);

        var useCase = new GetPeopleUseCase(repositoryMock.Object);

        var result = await useCase.ExecuteAsync();

        result.Select(x => x.Name).Should().Equal("Ana", "Bruno");
    }

    [Fact]
    public async Task DeletePerson_FirstDelete_ShouldSoftDelete()
    {
        var repositoryMock = new Mock<IPersonRepository>();
        var personId = Guid.NewGuid();
        var person = new Person
        {
            Id = personId,
            Name = "Ana",
            CreatedAtUtc = DateTime.UtcNow,
            DeletedAtUtc = null,
        };

        repositoryMock
            .Setup(x => x.GetByIdAsync(personId))
            .ReturnsAsync(person);

        var useCase = new DeletePersonUseCase(repositoryMock.Object);

        await useCase.ExecuteAsync(personId);

        repositoryMock.Verify(x => x.UpdateAsync(It.Is<Person>(p =>
            p.Id == personId &&
            p.DeletedAtUtc.HasValue)), Times.Once);
        repositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task DeletePerson_After30Days_ShouldHardDelete()
    {
        var repositoryMock = new Mock<IPersonRepository>();
        var personId = Guid.NewGuid();
        var person = new Person
        {
            Id = personId,
            Name = "Ana",
            CreatedAtUtc = DateTime.UtcNow.AddDays(-40),
            DeletedAtUtc = DateTime.UtcNow.AddDays(-31),
        };

        repositoryMock
            .Setup(x => x.GetByIdAsync(personId))
            .ReturnsAsync(person);

        var useCase = new DeletePersonUseCase(repositoryMock.Object);

        await useCase.ExecuteAsync(personId);

        repositoryMock.Verify(x => x.DeleteAsync(personId), Times.Once);
        repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Person>()), Times.Never);
    }

    [Fact]
    public async Task UpdatePerson_ShouldChangeOnlyName_KeepingCreatedAtUtc()
    {
        var repositoryMock = new Mock<IPersonRepository>();
        var personId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow.AddDays(-10);
        var person = new Person
        {
            Id = personId,
            Name = "Nome antigo",
            CreatedAtUtc = createdAt,
            DeletedAtUtc = null,
        };

        repositoryMock
            .Setup(x => x.GetByIdAsync(personId))
            .ReturnsAsync(person);

        var useCase = new UpdatePersonUseCase(repositoryMock.Object);

        var result = await useCase.ExecuteAsync(personId, new UpdatePersonRequest("Nome novo"));

        result.Name.Should().Be("Nome novo");
        result.CreatedAtUtc.Should().Be(createdAt);
        repositoryMock.Verify(x => x.UpdateAsync(It.Is<Person>(p =>
            p.Id == personId &&
            p.Name == "Nome novo" &&
            p.CreatedAtUtc == createdAt)), Times.Once);
    }
}