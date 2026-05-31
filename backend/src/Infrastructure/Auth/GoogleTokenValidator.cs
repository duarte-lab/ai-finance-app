using Application.Auth.Interfaces;
using System.Text.Json;

namespace Infrastructure.Auth;

public class GoogleTokenValidator : IGoogleTokenValidator
{
    private readonly HttpClient _httpClient;

    public GoogleTokenValidator(HttpClient httpClient)
    {
        _httpClient = httpClient;
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

        return new GoogleTokenPayload(
            GoogleId: sub.GetString()!,
            Email: email.GetString()!,
            Name: name.GetString()!);
    }
}
