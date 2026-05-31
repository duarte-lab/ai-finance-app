using Application.Accounts.Interfaces;
using Application.Auth.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using MongoDB.Driver;

namespace Infrastructure.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly IMongoCollection<Account> _accounts;
    private readonly ICurrentUserContext _currentUser;

    public AccountRepository(AppDbContext context, ICurrentUserContext currentUser)
    {
        _accounts = context.Accounts;
        _currentUser = currentUser;
    }

    private FilterDefinition<Account> TenantFilter()
        => _currentUser.TenantId.HasValue
            ? Builders<Account>.Filter.Eq(x => x.TenantId, _currentUser.TenantId.Value)
            : Builders<Account>.Filter.Empty;

    public async Task<IReadOnlyCollection<Account>> GetAllAsync(int? year = null, int? month = null)
    {
        var filter = TenantFilter();

        if (year.HasValue && month.HasValue)
        {
            var start = new DateTime(year.Value, month.Value, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = start.AddMonths(1);

            filter = Builders<Account>.Filter.And(
                filter,
                Builders<Account>.Filter.Gte(x => x.DueDate, start),
                Builders<Account>.Filter.Lt(x => x.DueDate, end));
        }

        var items = await _accounts
            .Find(filter)
            .SortBy(x => x.DueDate)
            .ToListAsync();

        return items;
    }

    public async Task<Account?> GetByIdAsync(Guid id)
    {
        var filter = Builders<Account>.Filter.And(
            TenantFilter(),
            Builders<Account>.Filter.Eq(x => x.Id, id));
        return await _accounts.Find(filter).FirstOrDefaultAsync();
    }

    public async Task CreateAsync(Account account)
    {
        await _accounts.InsertOneAsync(account);
    }

    public async Task UpdateAsync(Account account)
    {
        await _accounts.ReplaceOneAsync(x => x.Id == account.Id, account);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var result = await _accounts.DeleteOneAsync(x => x.Id == id);
        return result.DeletedCount > 0;
    }
}
