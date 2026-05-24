using System.Net;
using System.Net.Http.Json;
using Application.Accounts.DTOs;
using Application.People.DTOs;
using Xunit;

namespace API.IntegrationTests;

public class AccountsEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AccountsEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateApiClient();
    }

    [Fact]
    public async Task CreateAccount_ThenMarkAsPaid_ShouldReturnPaidAccount()
    {
        var createRequest = new CreateAccountRequest("Rent", 1500m, new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc));

        var createResponse = await _client.PostAsJsonAsync("/api/accounts", createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<AccountResponse>();
        Assert.NotNull(created);
        Assert.False(created!.Paid);

        var payResponse = await _client.PatchAsync($"/api/accounts/{created.Id}/pay", null);
        Assert.Equal(HttpStatusCode.OK, payResponse.StatusCode);

        var paid = await payResponse.Content.ReadFromJsonAsync<AccountResponse>();
        Assert.NotNull(paid);
        Assert.True(paid!.Paid);
    }

    [Fact]
    public async Task GetAccounts_FilterByMonth_ShouldReturnOnlyMatchingMonth()
    {
        await _client.PostAsJsonAsync("/api/accounts", new CreateAccountRequest("Internet", 120m, new DateTime(2026, 11, 3, 0, 0, 0, DateTimeKind.Utc)));
        await _client.PostAsJsonAsync("/api/accounts", new CreateAccountRequest("Gym", 80m, new DateTime(2026, 12, 3, 0, 0, 0, DateTimeKind.Utc)));

        var response = await _client.GetAsync("/api/accounts?year=2026&month=11");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var items = await response.Content.ReadFromJsonAsync<List<AccountResponse>>();
        Assert.NotNull(items);
        Assert.Single(items!);
        Assert.Equal("Internet", items[0].Name);
    }

    [Fact]
    public async Task CreateAccount_WithSharedParticipants_ShouldReturnParticipants()
    {
        var personOneResponse = await _client.PostAsJsonAsync("/api/people", new CreatePersonRequest("Ana"));
        var personTwoResponse = await _client.PostAsJsonAsync("/api/people", new CreatePersonRequest("Bruno"));

        Assert.Equal(HttpStatusCode.Created, personOneResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Created, personTwoResponse.StatusCode);

        var personOne = await personOneResponse.Content.ReadFromJsonAsync<PersonResponse>();
        var personTwo = await personTwoResponse.Content.ReadFromJsonAsync<PersonResponse>();

        var request = new CreateAccountRequest(
            "Rent",
            2500m,
            new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc),
            [
                new AccountParticipantRequest(personOne!.Id, 70m),
                new AccountParticipantRequest(personTwo!.Id, 30m),
            ]);

        var response = await _client.PostAsJsonAsync("/api/accounts", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<AccountResponse>();
        Assert.NotNull(payload);
        Assert.Equal(2, payload!.Participants.Count);
        Assert.Equal(100m, payload.Participants.Sum(x => x.Percentage));
    }

    [Fact]
    public async Task CreateAccount_WithInvalidParticipantsPercentage_ShouldReturnBadRequest()
    {
        var personOneResponse = await _client.PostAsJsonAsync("/api/people", new CreatePersonRequest("Ana"));
        var personTwoResponse = await _client.PostAsJsonAsync("/api/people", new CreatePersonRequest("Bruno"));

        var personOne = await personOneResponse.Content.ReadFromJsonAsync<PersonResponse>();
        var personTwo = await personTwoResponse.Content.ReadFromJsonAsync<PersonResponse>();

        var request = new CreateAccountRequest(
            "Rent",
            2500m,
            new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc),
            [
                new AccountParticipantRequest(personOne!.Id, 60m),
                new AccountParticipantRequest(personTwo!.Id, 20m),
            ]);

        var response = await _client.PostAsJsonAsync("/api/accounts", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateAccount_WithDuplicateParticipants_ShouldReturnBadRequest()
    {
        var personResponse = await _client.PostAsJsonAsync("/api/people", new CreatePersonRequest("Ana"));
        Assert.Equal(HttpStatusCode.Created, personResponse.StatusCode);

        var person = await personResponse.Content.ReadFromJsonAsync<PersonResponse>();
        Assert.NotNull(person);

        var request = new CreateAccountRequest(
            "Rent",
            2500m,
            new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc),
            [
                new AccountParticipantRequest(person!.Id, 50m),
                new AccountParticipantRequest(person.Id, 50m),
            ]);

        var response = await _client.PostAsJsonAsync("/api/accounts", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateAccount_WithNegativeAmount_ShouldReturnBadRequest()
    {
        var request = new CreateAccountRequest(
            "Invalid account",
            -10m,
            new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc));

        var response = await _client.PostAsJsonAsync("/api/accounts", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateDivisionParticipation_ShouldReturnUpdatedAccount()
    {
        var createResponse = await _client.PostAsJsonAsync(
            "/api/accounts",
            new CreateAccountRequest("Division account", 100m, new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc)));

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<AccountResponse>();
        Assert.NotNull(created);

        var updateResponse = await _client.PatchAsJsonAsync(
            $"/api/accounts/{created!.Id}/division-participation",
            new UpdateDivisionParticipationRequest(true));

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updated = await updateResponse.Content.ReadFromJsonAsync<AccountResponse>();
        Assert.NotNull(updated);
        Assert.True(updated!.ParticipatesInDivision);
    }
}
