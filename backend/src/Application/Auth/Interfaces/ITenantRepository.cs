using Domain.Entities;

namespace Application.Auth.Interfaces;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(Guid id);
    Task CreateAsync(Tenant tenant);
}
