using Application.MonthlyClosing.DTOs;
using Application.MonthlyClosing.Interfaces;

namespace Application.MonthlyClosing.UseCases;

public class GetMonthlyClosingUseCase
{
    private readonly IMonthlyClosingRepository _monthlyClosingRepository;

    public GetMonthlyClosingUseCase(IMonthlyClosingRepository monthlyClosingRepository)
    {
        _monthlyClosingRepository = monthlyClosingRepository;
    }

    public async Task<MonthlyClosingResponse?> ExecuteAsync(int year, int month)
    {
        if (year <= 0)
        {
            throw new ArgumentException("Year must be greater than 0.");
        }

        if (month is < 1 or > 12)
        {
            throw new ArgumentException("Month must be between 1 and 12.");
        }

        var closing = await _monthlyClosingRepository.GetActiveByYearMonthAsync(year, month);
        if (closing is null)
        {
            return null;
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