namespace Application.Accounts.DTOs;

public record CreateAccountRequest(string Name, decimal Amount, DateTime DueDate);
