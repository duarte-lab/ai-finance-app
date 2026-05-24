using Domain.Entities;

namespace Application.People.Interfaces;

public interface IPersonRepository
{
    Task<IReadOnlyCollection<Person>> GetAllAsync();
    Task<Person?> GetByIdAsync(Guid id);
    Task<IReadOnlyCollection<Person>> GetByIdsAsync(IReadOnlyCollection<Guid> ids);
    Task CreateAsync(Person person);
}