namespace Application.Accounts.DTOs;

public record AccountResponse(Guid Id, string Name, decimal Amount, DateTime DueDate, bool Paid);
