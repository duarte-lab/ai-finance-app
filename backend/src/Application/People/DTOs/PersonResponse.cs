namespace Application.People.DTOs;

public record PersonResponse(Guid Id, string Name, DateTime CreatedAtUtc);