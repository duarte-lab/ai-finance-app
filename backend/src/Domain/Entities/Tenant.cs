namespace Domain.Entities;

public class Tenant
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
