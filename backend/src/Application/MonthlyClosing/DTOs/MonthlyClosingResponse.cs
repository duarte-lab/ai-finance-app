namespace Application.MonthlyClosing.DTOs;

public record MonthlyClosingResponse(
    Guid Id,
    int Year,
    int Month,
    decimal TotalAmount,
    decimal AmountPerPerson,
    int AccountCount,
    int ParticipantCount,
    DateTime ClosedAtUtc);
