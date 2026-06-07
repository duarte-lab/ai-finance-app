using Domain.Entities;

namespace Application.People.DTOs;

public record PersonResponse(
	Guid Id,
	string Name,
	PersonType Type,
	DateTime CreatedAtUtc,
	DateTime? DeletedAtUtc);