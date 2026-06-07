using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Xunit;

namespace API.IntegrationTests;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private IContainer? _mongoContainer;
    private string? _connectionString;

    private const string TestSigningKey = "APP_LOCAL_SIGNING_KEY_CHANGE_ME_123456789";
    public static readonly Guid TestTenantId = Guid.NewGuid();
    public static readonly Guid TestUserId = Guid.NewGuid();

    public HttpClient CreateApiClient() => CreateClient();

    public HttpClient CreateAuthenticatedApiClient()
    {
        var client = CreateClient();
        var token = GenerateTestJwt(TestUserId, TestTenantId);
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static string GenerateTestJwt(Guid userId, Guid tenantId)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim("tenantId", tenantId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, "test@example.com"),
            new Claim(JwtRegisteredClaimNames.Name, "Test User"),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestSigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public Task InitializeAsync()
    {
        _mongoContainer = new ContainerBuilder()
            .WithImage("mongo:7.0")
            .WithPortBinding(27017, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(27017))
            .Build();

        return StartContainerAsync();
    }

    private async Task StartContainerAsync()
    {
        await _mongoContainer!.StartAsync();
        var port = _mongoContainer.GetMappedPublicPort(27017);
        _connectionString = $"mongodb://localhost:{port}";
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            var settings = new Dictionary<string, string?>
            {
                ["ConnectionStrings:MongoDb"] = _connectionString,
                ["MongoDb:DatabaseName"] = $"accounts-tests-{Guid.NewGuid():N}",
                ["Jwt:SigningKey"] = TestSigningKey,
            };

            configBuilder.AddInMemoryCollection(settings);
        });
    }

    public new async Task DisposeAsync()
    {
        if (_mongoContainer is not null)
        {
            await _mongoContainer.DisposeAsync();
        }

        await base.DisposeAsync();
    }
}
