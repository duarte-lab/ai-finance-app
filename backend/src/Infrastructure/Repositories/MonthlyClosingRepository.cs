using Application.MonthlyClosing.Interfaces;
using Domain.Entities;
using Infrastructure.Data;

namespace Infrastructure.Repositories;

public class MonthlyClosingRepository : IMonthlyClosingRepository
{
    private readonly MongoDB.Driver.IMongoCollection<MonthlyClosing> _closings;

    public MonthlyClosingRepository(AppDbContext context)
    {
        _closings = context.MonthlyClosings;
    }

    public async Task CreateAsync(MonthlyClosing closing)
    {
        await _closings.InsertOneAsync(closing);
    }
}
