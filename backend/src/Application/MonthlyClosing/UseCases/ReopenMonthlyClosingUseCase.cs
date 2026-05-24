using Application.Accounts.Interfaces;
using Application.MonthlyClosing.DTOs;
using Application.MonthlyClosing.Interfaces;

namespace Application.MonthlyClosing.UseCases;

public class ReopenMonthlyClosingUseCase
{
    private readonly IAccountRepository _accountRepository;
    private readonly IMonthlyClosingRepository _monthlyClosingRepository;

    public ReopenMonthlyClosingUseCase(
        IAccountRepository accountRepository,
        IMonthlyClosingRepository monthlyClosingRepository)
    {
        _accountRepository = accountRepository;
        _monthlyClosingRepository = monthlyClosingRepository;
    }

    public async Task<MonthlyClosingResponse> ExecuteAsync(ReopenMonthlyClosingRequest request)
    {
        if (request.Year <= 0)
        {
            throw new ArgumentException("Year must be greater than 0.");
        }

        if (request.Month is < 1 or > 12)
        {
            throw new ArgumentException("Month must be between 1 and 12.");
        }

        var closing = await _monthlyClosingRepository.GetByYearMonthAsync(request.Year, request.Month);
        if (closing is null)
        {
            throw new InvalidOperationException("No active closing found for selected month.");
        }

        var monthAccounts = await _accountRepository.GetAllAsync(request.Year, request.Month);
        var lookup = monthAccounts.ToDictionary(x => x.Id, x => x);

        foreach (var accountId in closing.AccountIds)
        {
            if (!lookup.TryGetValue(accountId, out var account))
            {
                continue;
            }

            account.Paid = false;
            await _accountRepository.UpdateAsync(account);
        }

        closing.ReopenedAtUtc = DateTime.UtcNow;
        await _monthlyClosingRepository.UpdateAsync(closing);

        return new MonthlyClosingResponse(
            Id: closing.Id,
            Year: closing.Year,
            Month: closing.Month,
            TotalAmount: closing.TotalAmount,
            AmountPerPerson: closing.AmountPerPerson,
            AccountCount: closing.AccountIds.Count,
            ParticipantCount: closing.Participants.Count,
            ClosedAtUtc: closing.ClosedAtUtc,
            IsReopened: true,
            ReopenedAtUtc: closing.ReopenedAtUtc);
    }
}