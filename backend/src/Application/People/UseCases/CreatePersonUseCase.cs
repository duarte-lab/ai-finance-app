using Application.Auth.Interfaces;
using Application.People.DTOs;
using Application.People.Interfaces;
using Domain.Entities;

namespace Application.People.UseCases;

public class CreatePersonUseCase
{
    private const int MaxNameLength = 50;
    private readonly IPersonRepository _repository;
    private readonly ICurrentUserContext _currentUser;

    public CreatePersonUseCase(IPersonRepository repository, ICurrentUserContext currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<PersonResponse> ExecuteAsync(CreatePersonRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("Name is required.", nameof(request));
        }

        var trimmedName = request.Name.Trim();
        if (trimmedName.Length > MaxNameLength)
        {
            throw new ArgumentException($"Name must have at most {MaxNameLength} characters.", nameof(request));
        }

        var person = new Person
        {
            Id = Guid.NewGuid(),
            TenantId = _currentUser.TenantId ?? Guid.Empty,
            Name = trimmedName,
            Type = PersonType.Guest,
            CreatedAtUtc = DateTime.UtcNow,
            DeletedAtUtc = null,
        };

        await _repository.CreateAsync(person);

        return new PersonResponse(person.Id, person.Name, person.Type, person.CreatedAtUtc, person.DeletedAtUtc);
    }
}