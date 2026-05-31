using Application.Auth.Interfaces;
using Application.MonthlyClosing.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using MongoDB.Driver;

namespace Infrastructure.Repositories;

public class MonthlyClosingRepository : IMonthlyClosingRepository
{
    private readonly MongoDB.Driver.IMongoCollection<MonthlyClosing> _closings;
    private readonly ICurrentUserContext _currentUser;

    public MonthlyClosingRepository(AppDbContext context, ICurrentUserContext currentUser)
    {
        _closings = context.MonthlyClosings;
        _currentUser = currentUser;
    }

    private FilterDefinition<MonthlyClosing> TenantFilter()
        => _currentUser.TenantId.HasValue
            ? Builders<MonthlyClosing>.Filter.Eq(x => x.TenantId, _currentUser.TenantId.Value)
            : Builders<MonthlyClosing>.Filter.Empty;

    public async Task<MonthlyClosing?> GetActiveByYearMonthAsync(int year, int month)
    {
        var filter = Builders<MonthlyClosing>.Filter.And(
            TenantFilter(),
            Builders<MonthlyClosing>.Filter.Eq(x => x.Year, year),
            Builders<MonthlyClosing>.Filter.Eq(x => x.Month, month),
            Builders<MonthlyClosing>.Filter.Eq(x => x.ReopenedAtUtc, null));

        return await _closings
            .Find(filter)
            .SortByDescending(x => x.ClosedAtUtc)
            .FirstOrDefaultAsync();
    }

    public async Task<MonthlyClosing?> GetLatestByYearMonthAsync(int year, int month)
    {
        var filter = Builders<MonthlyClosing>.Filter.And(
            TenantFilter(),
            Builders<MonthlyClosing>.Filter.Eq(x => x.Year, year),
            Builders<MonthlyClosing>.Filter.Eq(x => x.Month, month));

        return await _closings
            .Find(filter)
            .SortByDescending(x => x.ClosedAtUtc)
            .FirstOrDefaultAsync();
    }

    public async Task CreateAsync(MonthlyClosing closing)
    {
        await _closings.InsertOneAsync(closing);
    }

    public async Task UpdateAsync(MonthlyClosing closing)
    {
        await _closings.ReplaceOneAsync(x => x.Id == closing.Id, closing);
    }
}
