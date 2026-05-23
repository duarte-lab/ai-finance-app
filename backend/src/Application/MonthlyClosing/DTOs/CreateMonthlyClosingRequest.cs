namespace Application.MonthlyClosing.DTOs;

public record CreateMonthlyClosingRequest(
    int Year,
    int Month,
    IReadOnlyCollection<Guid> AccountIds,
    IReadOnlyCollection<string> Participants);
