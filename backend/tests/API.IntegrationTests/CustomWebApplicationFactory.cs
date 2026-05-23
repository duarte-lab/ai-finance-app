using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace API.IntegrationTests;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private IContainer? _mongoContainer;
    private string? _connectionString;

    public HttpClient CreateApiClient() => CreateClient();

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
                ["MongoDb:DatabaseName"] = $"accounts-tests-{Guid.NewGuid():N}"
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
