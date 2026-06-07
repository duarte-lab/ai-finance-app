using Application.Auth.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using MongoDB.Driver;

namespace Infrastructure.Auth;

public class OwnerPersonProvisioner : IOwnerPersonProvisioner
{
    private readonly IMongoCollection<Person> _people;

    public OwnerPersonProvisioner(AppDbContext context)
    {
        _people = context.People;
    }

    public async Task EnsureOwnerPersonAsync(User user)
    {
        var existing = await _people
            .Find(x => x.TenantId == user.TenantId && x.UserId == user.Id && x.Type == PersonType.Owner)
            .FirstOrDefaultAsync();

        if (existing is not null)
            return;

        var ownerByTenant = await _people
            .Find(x => x.TenantId == user.TenantId && x.Type == PersonType.Owner && x.DeletedAtUtc == null)
            .FirstOrDefaultAsync();

        if (ownerByTenant is not null)
        {
            ownerByTenant.UserId = user.Id;
            ownerByTenant.Name = user.Name;
            await _people.ReplaceOneAsync(x => x.Id == ownerByTenant.Id, ownerByTenant);
            return;
        }

        await _people.InsertOneAsync(new Person
        {
            Id = Guid.NewGuid(),
            TenantId = user.TenantId,
            UserId = user.Id,
            Name = user.Name,
            Type = PersonType.Owner,
            CreatedAtUtc = DateTime.UtcNow,
            DeletedAtUtc = null,
        });
    }
}
