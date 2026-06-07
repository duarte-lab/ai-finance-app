namespace Application.Auth.DTOs;

public record RegisterRequest(string Name, string Email, string Password);

public record LoginRequest(string Email, string Password);

public record GoogleAuthRequest(string IdToken);

public record RefreshTokenRequest(string RefreshToken);

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    Guid UserId,
    Guid TenantId,
    string Email,
    string Name);
