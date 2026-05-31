namespace Application.Auth.DTOs;

public record GoogleAuthRequest(string IdToken);

public record AuthResponse(
    string AccessToken,
    Guid UserId,
    Guid TenantId,
    string Email,
    string Name);
