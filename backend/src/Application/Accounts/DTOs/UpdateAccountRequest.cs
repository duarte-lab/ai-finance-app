namespace Application.Accounts.DTOs;

public record UpdateAccountRequest(
	string Name,
	decimal Amount,
	DateTime DueDate,
	bool Paid,
	IReadOnlyCollection<AccountParticipantRequest>? Participants = null,
	bool ParticipatesInDivision = false);
