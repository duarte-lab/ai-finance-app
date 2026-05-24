namespace Application.Accounts.DTOs;

public record AccountParticipantResponse(Guid PersonId, decimal Percentage);

public record AccountResponse(
	Guid Id,
	string Name,
	decimal Amount,
	DateTime DueDate,
	bool Paid,
	IReadOnlyCollection<AccountParticipantResponse> Participants);
