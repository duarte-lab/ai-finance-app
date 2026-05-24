using Application.MonthlyClosing.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using MongoDB.Driver;

namespace Infrastructure.Repositories;

public class MonthlyClosingRepository : IMonthlyClosingRepository
{
    private readonly MongoDB.Driver.IMongoCollection<MonthlyClosing> _closings;

    public MonthlyClosingRepository(AppDbContext context)
    {
        _closings = context.MonthlyClosings;
    }

    public async Task<MonthlyClosing?> GetByYearMonthAsync(int year, int month)
    {
        return await _closings
            .Find(x => x.Year == year && x.Month == month && x.ReopenedAtUtc == null)
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
