using Domain.Entities;

namespace Application.Accounts.Interfaces;

public interface IAccountRepository
{
    Task<IReadOnlyCollection<Account>> GetAllAsync(int? year = null, int? month = null);
    Task<Account?> GetByIdAsync(Guid id);
    Task CreateAsync(Account account);
    Task UpdateAsync(Account account);
    Task<bool> DeleteAsync(Guid id);
}
