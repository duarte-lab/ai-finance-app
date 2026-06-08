using Application.Accounts.Interfaces;
using Application.Dashboard.DTOs;

namespace Application.Dashboard.UseCases;

public class GetDashboardSummaryUseCase
{
    private readonly IAccountRepository _accountRepository;

    public GetDashboardSummaryUseCase(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<DashboardSummaryResponse> ExecuteAsync(int year, int month)
    {
        var accounts = await _accountRepository.GetAllAsync(year, month);

        var paidAccounts = accounts.Where(x => x.Paid).ToList();
        var pendingAccounts = accounts.Where(x => !x.Paid).ToList();

        var paidAmount = paidAccounts.Sum(x => x.Amount);
        var pendingAmount = pendingAccounts.Sum(x => x.Amount);

        var paidSeries = paidAccounts
            .GroupBy(a => a.DueDate.ToString("yyyy-MM-dd"))
            .Select(g => new DashboardCategoryPointResponse(g.Key, g.Sum(a => a.Amount), g.Count()))
            .OrderBy(p => p.Label)
            .ToList();

        var lastSixMonths = new List<DashboardMonthlyTotalResponse>();
        for (var i = 5; i >= 0; i--)
        {
            var target = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-i);
            var monthAccounts = (target.Year == year && target.Month == month)
                ? accounts
                : await _accountRepository.GetAllAsync(target.Year, target.Month);

            lastSixMonths.Add(new DashboardMonthlyTotalResponse(
                target.Year, target.Month, monthAccounts.Sum(a => a.Amount)));
        }

        return new DashboardSummaryResponse(
            Year: year,
            Month: month,
            TotalAmount: accounts.Sum(x => x.Amount),
            PaidAmount: paidAmount,
            PendingAmount: pendingAmount,
            TotalCount: accounts.Count,
            PaidCount: paidAccounts.Count,
            PendingCount: pendingAccounts.Count,
            Chart:
            [
                new DashboardCategoryPointResponse("Paid", paidAmount, paidAccounts.Count),
                new DashboardCategoryPointResponse("Pending", pendingAmount, pendingAccounts.Count)
            ],
            PaidSeries: paidSeries,
            LastSixMonths: lastSixMonths);
    }
}
