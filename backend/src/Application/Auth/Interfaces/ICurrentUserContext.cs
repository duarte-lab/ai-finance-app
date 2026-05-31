namespace Application.Auth.Interfaces;

public interface ICurrentUserContext
{
    Guid? TenantId { get; }
    Guid? UserId { get; }
}
