using Application.Auth.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Infrastructure.Auth;

public class GoogleTokenValidator : IGoogleTokenValidator
{
    private readonly HttpClient _httpClient;
    private readonly string? _googleClientId;

    public GoogleTokenValidator(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _googleClientId = configuration["Google:ClientId"];
    }

    public async Task<GoogleTokenPayload?> ValidateAsync(string idToken)
    {
        var response = await _httpClient.GetAsync(
            $"https://oauth2.googleapis.com/tokeninfo?id_token={Uri.EscapeDataString(idToken)}");

        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (!root.TryGetProperty("sub", out var sub) ||
            !root.TryGetProperty("email", out var email) ||
            !root.TryGetProperty("name", out var name))
            return null;

        if (!string.IsNullOrWhiteSpace(_googleClientId) &&
            (!root.TryGetProperty("aud", out var aud) || aud.GetString() != _googleClientId))
            return null;

        if (!root.TryGetProperty("exp", out var exp) ||
            !long.TryParse(exp.GetString(), out var expUnix) ||
            DateTimeOffset.FromUnixTimeSeconds(expUnix) <= DateTimeOffset.UtcNow)
            return null;

        return new GoogleTokenPayload(
            GoogleId: sub.GetString()!,
            Email: email.GetString()!,
            Name: name.GetString()!);
    }
}
