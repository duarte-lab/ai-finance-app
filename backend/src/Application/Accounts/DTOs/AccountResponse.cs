namespace Application.Accounts.DTOs;

public record AccountResponse(
	Guid Id,
	string Name,
	decimal Amount,
	DateTime DueDate,
	DateTime CreatedAtUtc,
	bool Paid,
	bool ParticipatesInDivision);

public record UpdateDivisionParticipationRequest(bool ParticipatesInDivision);
