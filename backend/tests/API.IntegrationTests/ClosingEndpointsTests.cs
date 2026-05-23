using System.Net;
using System.Net.Http.Json;
using Application.Accounts.DTOs;
using Application.MonthlyClosing.DTOs;
using Xunit;

namespace API.IntegrationTests;

public class ClosingEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ClosingEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateApiClient();
    }

    [Fact]
    public async Task CreateClosing_WithMultipleAccounts_ShouldReturnCalculatedDivision()
    {
        var first = await _client.PostAsJsonAsync(
            "/api/accounts",
            new CreateAccountRequest("Rent", 1000m, new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc)));
        var second = await _client.PostAsJsonAsync(
            "/api/accounts",
            new CreateAccountRequest("Internet", 500m, new DateTime(2026, 5, 15, 0, 0, 0, DateTimeKind.Utc)));

        var accountOne = await first.Content.ReadFromJsonAsync<AccountResponse>();
        var accountTwo = await second.Content.ReadFromJsonAsync<AccountResponse>();

        Assert.NotNull(accountOne);
        Assert.NotNull(accountTwo);

        var closingRequest = new CreateMonthlyClosingRequest(
            Year: 2026,
            Month: 5,
            AccountIds: [accountOne!.Id, accountTwo!.Id],
            Participants: ["Ana", "Bruno"]);

        var response = await _client.PostAsJsonAsync("/closing", closingRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<MonthlyClosingResponse>();
        Assert.NotNull(payload);
        Assert.Equal(1500m, payload!.TotalAmount);
        Assert.Equal(750m, payload.AmountPerPerson);
        Assert.Equal(2, payload.AccountCount);
        Assert.Equal(2, payload.ParticipantCount);
    }

    [Fact]
    public async Task CreateClosing_WithoutAccounts_ShouldReturnBadRequest()
    {
        var closingRequest = new CreateMonthlyClosingRequest(
            Year: 2026,
            Month: 5,
            AccountIds: [],
            Participants: ["Ana", "Bruno"]);

        var response = await _client.PostAsJsonAsync("/closing", closingRequest);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
