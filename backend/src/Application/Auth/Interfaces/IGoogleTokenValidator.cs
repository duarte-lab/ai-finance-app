namespace Application.Auth.Interfaces;

public record GoogleTokenPayload(
    string GoogleId,
    string Email,
    string Name);

public interface IGoogleTokenValidator
{
    Task<GoogleTokenPayload?> ValidateAsync(string idToken);
}
