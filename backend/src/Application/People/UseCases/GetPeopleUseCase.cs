using Application.People.DTOs;
using Application.People.Interfaces;

namespace Application.People.UseCases;

public class GetPeopleUseCase
{
    private readonly IPersonRepository _repository;

    public GetPeopleUseCase(IPersonRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyCollection<PersonResponse>> ExecuteAsync()
    {
        var people = await _repository.GetAllAsync();
        return people
            .OrderBy(x => x.Name)
            .Select(x => new PersonResponse(x.Id, x.Name, x.CreatedAtUtc))
            .ToList();
    }
}