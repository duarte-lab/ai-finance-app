using Application.Auth.Interfaces;
using Application.People.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using MongoDB.Driver;

namespace Infrastructure.Repositories;

public class PersonRepository : IPersonRepository
{
    private readonly IMongoCollection<Person> _people;
    private readonly ICurrentUserContext _currentUser;

    public PersonRepository(AppDbContext context, ICurrentUserContext currentUser)
    {
        _people = context.People;
        _currentUser = currentUser;
    }

    private FilterDefinition<Person> TenantFilter()
        => _currentUser.TenantId.HasValue
            ? Builders<Person>.Filter.Eq(x => x.TenantId, _currentUser.TenantId.Value)
            : Builders<Person>.Filter.Empty;

    public async Task<IReadOnlyCollection<Person>> GetAllAsync()
    {
        var filter = Builders<Person>.Filter.And(
            TenantFilter(),
            Builders<Person>.Filter.Eq(x => x.DeletedAtUtc, null));

        return await _people
            .Find(filter)
            .SortBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<Person?> GetByIdAsync(Guid id)
    {
        var filter = Builders<Person>.Filter.And(
            TenantFilter(),
            Builders<Person>.Filter.Eq(x => x.Id, id));
        return await _people.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyCollection<Person>> GetByIdsAsync(IReadOnlyCollection<Guid> ids)
    {
        if (ids.Count == 0)
        {
            return [];
        }

        var filter = Builders<Person>.Filter.And(
            TenantFilter(),
            Builders<Person>.Filter.In(x => x.Id, ids),
            Builders<Person>.Filter.Eq(x => x.DeletedAtUtc, null));

        return await _people.Find(filter).ToListAsync();
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