namespace Application.MonthlyClosing.DTOs;

public record MonthlyClosingResponse(
    Guid Id,
    int Year,
    int Month,
    decimal TotalAmount,
    decimal AmountPerPerson,
    int AccountCount,
    int ParticipantCount,
    DateTime ClosedAtUtc,
    bool IsReopened = false,
    DateTime? ReopenedAtUtc = null,
    IReadOnlyCollection<string>? Participants = null);

public record ReopenMonthlyClosingRequest(int Year, int Month);
