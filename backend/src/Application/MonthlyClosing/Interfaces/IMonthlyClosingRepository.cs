namespace Application.MonthlyClosing.Interfaces;

public interface IMonthlyClosingRepository
{
    Task<Domain.Entities.MonthlyClosing?> GetActiveByYearMonthAsync(int year, int month);
    Task<Domain.Entities.MonthlyClosing?> GetLatestByYearMonthAsync(int year, int month);
    Task CreateAsync(Domain.Entities.MonthlyClosing closing);
    Task UpdateAsync(Domain.Entities.MonthlyClosing closing);
}
