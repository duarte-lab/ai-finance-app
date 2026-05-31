namespace Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public required string GoogleId { get; set; }
    public required string Email { get; set; }
    public required string Name { get; set; }
    public Guid TenantId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
