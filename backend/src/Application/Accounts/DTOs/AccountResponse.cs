namespace Application.Accounts.DTOs;

public record AccountParticipantResponse(Guid PersonId);

public record AccountResponse(
	Guid Id,
	string Name,
	decimal Amount,
	DateTime DueDate,
	DateTime CreatedAtUtc,
	bool Paid,
	bool ParticipatesInDivision,
	IReadOnlyCollection<AccountParticipantResponse> Participants);

public record UpdateDivisionParticipationRequest(bool ParticipatesInDivision);
