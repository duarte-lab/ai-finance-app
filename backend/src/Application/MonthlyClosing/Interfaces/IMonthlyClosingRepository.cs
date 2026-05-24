namespace Application.MonthlyClosing.Interfaces;

public interface IMonthlyClosingRepository
{
    Task<Domain.Entities.MonthlyClosing?> GetByYearMonthAsync(int year, int month);
    Task CreateAsync(Domain.Entities.MonthlyClosing closing);
    Task UpdateAsync(Domain.Entities.MonthlyClosing closing);
}
