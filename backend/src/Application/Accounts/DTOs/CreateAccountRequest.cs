namespace Application.Accounts.DTOs;

public record AccountParticipantRequest(Guid PersonId);

public record CreateAccountRequest(
	string Name,
	decimal Amount,
	DateTime DueDate,
	IReadOnlyCollection<AccountParticipantRequest>? Participants = null,
	bool ParticipatesInDivision = false);
