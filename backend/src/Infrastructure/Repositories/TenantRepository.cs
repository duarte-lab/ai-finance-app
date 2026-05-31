using Application.Auth.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using MongoDB.Driver;

namespace Infrastructure.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly IMongoCollection<Tenant> _tenants;

    public TenantRepository(AppDbContext context)
    {
        _tenants = context.Tenants;
    }

    public async Task<Tenant?> GetByIdAsync(Guid id)
        => await _tenants.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(Tenant tenant)
        => await _tenants.InsertOneAsync(tenant);
}
