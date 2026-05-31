namespace Application.Auth.Interfaces;

public interface IJwtGenerator
{
    string Generate(Guid userId, Guid tenantId, string email, string name);
}
