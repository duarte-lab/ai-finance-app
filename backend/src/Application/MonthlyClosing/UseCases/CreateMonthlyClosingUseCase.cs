using Application.Accounts.Interfaces;
using Application.MonthlyClosing.DTOs;
using Application.MonthlyClosing.Interfaces;
using Domain.Entities;

namespace Application.MonthlyClosing.UseCases;

public class CreateMonthlyClosingUseCase
{
    private readonly IAccountRepository _accountRepository;
    private readonly IMonthlyClosingRepository _monthlyClosingRepository;

    public CreateMonthlyClosingUseCase(
        IAccountRepository accountRepository,
        IMonthlyClosingRepository monthlyClosingRepository)
    {
        _accountRepository = accountRepository;
        _monthlyClosingRepository = monthlyClosingRepository;
    }

    public async Task<MonthlyClosingResponse> ExecuteAsync(CreateMonthlyClosingRequest request)
    {
        if (request.Year <= 0)
        {
            throw new ArgumentException("Year must be greater than 0.");
        }

        if (request.Month is < 1 or > 12)
        {
            throw new ArgumentException("Month must be between 1 and 12.");
        }

        var accountIds = request.AccountIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        var participants = request.Participants
            .Select(name => name.Trim())
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (participants.Count == 0)
        {
            throw new ArgumentException("At least one participant is required.");
        }

        var monthAccounts = await _accountRepository.GetAllAsync(request.Year, request.Month);
        var unpaidAccounts = monthAccounts.Where(account => !account.Paid).ToList();
        var unpaidLookup = unpaidAccounts.ToDictionary(account => account.Id, account => account);

        if (accountIds.Any(id => !unpaidLookup.ContainsKey(id)))
        {
            throw new InvalidOperationException("Only unpaid accounts from the selected month are allowed.");
        }

        var autoIncludedIds = unpaidAccounts
            .Where(account => account.ParticipatesInDivision)
            .Select(account => account.Id)
            .ToList();

        var selectedIds = autoIncludedIds
            .Union(accountIds)
            .Distinct()
            .ToList();

        if (selectedIds.Count == 0)
        {
            throw new InvalidOperationException("At least one account must be selected.");
        }

        var selectedAccounts = selectedIds.Select(id => unpaidLookup[id]).ToList();
        var totalAmount = selectedAccounts.Sum(account => account.Amount);
        var amountPerPerson = decimal.Round(
            totalAmount / participants.Count,
            2,
            MidpointRounding.AwayFromZero);

        var closing = new Domain.Entities.MonthlyClosing
        {
            Id = Guid.NewGuid(),
            Year = request.Year,
            Month = request.Month,
            ClosedAtUtc = DateTime.UtcNow,
            ReopenedAtUtc = null,
            AccountIds = selectedAccounts.Select(account => account.Id).ToList(),
            Participants = participants,
            TotalAmount = totalAmount,
            AmountPerPerson = amountPerPerson,
        };

        foreach (var account in selectedAccounts)
        {
            account.MarkAsPaid();
            await _accountRepository.UpdateAsync(account);
        }

        await _monthlyClosingRepository.CreateAsync(closing);

        return new MonthlyClosingResponse(
            Id: closing.Id,
            Year: closing.Year,
            Month: closing.Month,
            TotalAmount: closing.TotalAmount,
            AmountPerPerson: closing.AmountPerPerson,
            AccountCount: closing.AccountIds.Count,
            ParticipantCount: closing.Participants.Count,
            ClosedAtUtc: closing.ClosedAtUtc,
            IsReopened: false,
            ReopenedAtUtc: null);
    }
}
