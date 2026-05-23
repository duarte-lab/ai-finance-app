namespace Application.MonthlyClosing.Interfaces;

public interface IMonthlyClosingRepository
{
    Task CreateAsync(Domain.Entities.MonthlyClosing closing);
}
