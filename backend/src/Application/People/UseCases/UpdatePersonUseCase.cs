using Application.People.DTOs;
using Application.People.Interfaces;

namespace Application.People.UseCases;

public class UpdatePersonUseCase
{
    private const int MaxNameLength = 50;
    private readonly IPersonRepository _repository;

    public UpdatePersonUseCase(IPersonRepository repository)
    {
        _repository = repository;
    }

    public async Task<PersonResponse> ExecuteAsync(Guid id, UpdatePersonRequest request)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Person id is required.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("Name is required.", nameof(request));
        }

        var trimmedName = request.Name.Trim();
        if (trimmedName.Length > MaxNameLength)
        {
            throw new ArgumentException($"Name must have at most {MaxNameLength} characters.", nameof(request));
        }

        var person = await _repository.GetByIdAsync(id);
        if (person is null || person.DeletedAtUtc is not null)
        {
            throw new KeyNotFoundException("Person not found.");
        }

        person.Name = trimmedName;
        await _repository.UpdateAsync(person);

        return new PersonResponse(person.Id, person.Name, person.Type, person.CreatedAtUtc, person.DeletedAtUtc);
    }
}
