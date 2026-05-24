using Application.People.DTOs;
using Application.People.Interfaces;
using Domain.Entities;

namespace Application.People.UseCases;

public class CreatePersonUseCase
{
    private readonly IPersonRepository _repository;

    public CreatePersonUseCase(IPersonRepository repository)
    {
        _repository = repository;
    }

    public async Task<PersonResponse> ExecuteAsync(CreatePersonRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("Name is required.", nameof(request));
        }

        var person = new Person
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
        };

        await _repository.CreateAsync(person);

        return new PersonResponse(person.Id, person.Name, person.CreatedAtUtc);
    }
}