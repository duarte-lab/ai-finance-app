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
    public async Task GetPeople_ShouldReturnAlphabeticalOrder()
    {
        var repositoryMock = new Mock<IPersonRepository>();
        repositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(
            [
                new Person { Id = Guid.NewGuid(), Name = "Bruno", CreatedAtUtc = DateTime.UtcNow },
                new Person { Id = Guid.NewGuid(), Name = "Ana", CreatedAtUtc = DateTime.UtcNow },
            ]);

        var useCase = new GetPeopleUseCase(repositoryMock.Object);

        var result = await useCase.ExecuteAsync();

        result.Select(x => x.Name).Should().Equal("Ana", "Bruno");
    }
}