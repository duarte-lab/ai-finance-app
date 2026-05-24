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

        var existingClosing = await _monthlyClosingRepository.GetActiveByYearMonthAsync(request.Year, request.Month);
        if (existingClosing is not null)
        {
            throw new InvalidOperationException("Selected month is already closed. Reopen it before closing again.");
        }

        var latestClosing = await _monthlyClosingRepository.GetLatestByYearMonthAsync(request.Year, request.Month);

        var monthAccounts = await _accountRepository.GetAllAsync(request.Year, request.Month);
        var monthLookup = monthAccounts.ToDictionary(account => account.Id, account => account);

        if (accountIds.Any(id => !monthLookup.ContainsKey(id)))
        {
            throw new InvalidOperationException("Only accounts from the selected month are allowed.");
        }

        var autoIncludedIds = monthAccounts
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

        var selectedAccounts = selectedIds.Select(id => monthLookup[id]).ToList();
        var totalAmount = selectedAccounts.Sum(account => account.Amount);
        var amountPerPerson = decimal.Round(
            totalAmount / participants.Count,
            2,
            MidpointRounding.AwayFromZero);

        var shouldReuseLatestClosing = latestClosing is not null && latestClosing.ReopenedAtUtc is not null;

        Domain.Entities.MonthlyClosing closing;
        if (shouldReuseLatestClosing)
        {
            closing = latestClosing!;
        }
        else
        {
            closing = new Domain.Entities.MonthlyClosing
            {
                Id = Guid.NewGuid(),
                Year = request.Year,
                Month = request.Month,
                AccountIds = [],
                Participants = [],
            };
        }

        closing.Year = request.Year;
        closing.Month = request.Month;
        closing.ClosedAtUtc = DateTime.UtcNow;
        closing.ReopenedAtUtc = null;
        closing.AccountIds = selectedAccounts.Select(account => account.Id).ToList();
        closing.Participants = participants;
        closing.TotalAmount = totalAmount;
        closing.AmountPerPerson = amountPerPerson;

        foreach (var account in selectedAccounts)
        {
            account.MarkAsPaid();
            await _accountRepository.UpdateAsync(account);
        }

        if (shouldReuseLatestClosing)
        {
            await _monthlyClosingRepository.UpdateAsync(closing);
        }
        else
        {
            await _monthlyClosingRepository.CreateAsync(closing);
        }

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
            ReopenedAtUtc: null,
            Participants: closing.Participants.ToList());
    }
}
