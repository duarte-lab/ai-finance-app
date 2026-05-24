using Application.People.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using MongoDB.Driver;

namespace Infrastructure.Repositories;

public class PersonRepository : IPersonRepository
{
    private readonly IMongoCollection<Person> _people;

    public PersonRepository(AppDbContext context)
    {
        _people = context.People;
    }

    public async Task<IReadOnlyCollection<Person>> GetAllAsync()
    {
        return await _people
            .Find(x => x.DeletedAtUtc == null)
            .SortBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<Person?> GetByIdAsync(Guid id)
    {
        return await _people.Find(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyCollection<Person>> GetByIdsAsync(IReadOnlyCollection<Guid> ids)
    {
        if (ids.Count == 0)
        {
            return [];
        }

        return await _people
            .Find(x => ids.Contains(x.Id) && x.DeletedAtUtc == null)
            .ToListAsync();
    }

    public async Task CreateAsync(Person person)
    {
        await _people.InsertOneAsync(person);
    }

    public async Task UpdateAsync(Person person)
    {
        await _people.ReplaceOneAsync(x => x.Id == person.Id, person);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _people.DeleteOneAsync(x => x.Id == id);
    }
}