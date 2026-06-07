using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.Auth.DTOs;
using Application.People.DTOs;
using Xunit;

namespace API.IntegrationTests;

public class AuthEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateApiClient();
    }

    [Fact]
    public async Task Register_ThenLogin_AndRefresh_ShouldSucceed()
    {
        var email = $"ana-{Guid.NewGuid():N}@example.com";

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest("Ana", email, "password-123"));
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var registered = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(registered);
        Assert.False(string.IsNullOrWhiteSpace(registered!.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(registered.RefreshToken));

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "password-123"));
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var login = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(login);

        var refreshResponse = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest(login!.RefreshToken));
        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);

        var refreshed = await refreshResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(refreshed);
        Assert.NotEqual(login.RefreshToken, refreshed!.RefreshToken);
    }

    [Fact]
    public async Task Login_InvalidPassword_ShouldReturnUnauthorized()
    {
        var email = $"bruno-{Guid.NewGuid():N}@example.com";
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest("Bruno", email, "password-123"));
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "wrong-password"));

        Assert.Equal(HttpStatusCode.Unauthorized, loginResponse.StatusCode);
    }

    [Fact]
    public async Task Refresh_InvalidToken_ShouldReturnUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest("invalid-token"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RegisteredOwner_ShouldNotBeDeletable()
    {
        var email = $"owner-{Guid.NewGuid():N}@example.com";
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest("Owner User", email, "password-123"));
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var auth = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.AccessToken);

        var peopleResponse = await _client.GetAsync("/api/people");
        Assert.Equal(HttpStatusCode.OK, peopleResponse.StatusCode);

        var people = await peopleResponse.Content.ReadFromJsonAsync<List<PersonResponse>>();
        Assert.NotNull(people);

        var owner = Assert.Single(people!, p => p.Type == Domain.Entities.PersonType.Owner);

        var deleteResponse = await _client.DeleteAsync($"/api/people/{owner.Id}");
        Assert.Equal(HttpStatusCode.Conflict, deleteResponse.StatusCode);
    }
}
