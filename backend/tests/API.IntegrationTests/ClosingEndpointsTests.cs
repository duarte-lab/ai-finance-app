using System.Net;
using System.Net.Http.Json;
using Application.Accounts.DTOs;
using Application.MonthlyClosing.DTOs;
using Xunit;

namespace API.IntegrationTests;

public class ClosingEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    private static (int Year, int Month) BuildUniquePeriod()
    {
        var hash = Math.Abs(Guid.NewGuid().GetHashCode());
        var year = 3000 + (hash % 6000);
        var month = (hash % 12) + 1;
        return (year, month);
    }

    public ClosingEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateApiClient();
    }

    [Fact]
    public async Task CreateClosing_WithMultipleAccounts_ShouldReturnCalculatedDivision()
    {
        var (year, month) = BuildUniquePeriod();

        var first = await _client.PostAsJsonAsync(
            "/api/accounts",
            new CreateAccountRequest("Rent", 1000m, new DateTime(year, month, 10, 0, 0, 0, DateTimeKind.Utc)));
        var second = await _client.PostAsJsonAsync(
            "/api/accounts",
            new CreateAccountRequest("Internet", 500m, new DateTime(year, month, 15, 0, 0, 0, DateTimeKind.Utc)));

        var accountOne = await first.Content.ReadFromJsonAsync<AccountResponse>();
        var accountTwo = await second.Content.ReadFromJsonAsync<AccountResponse>();

        Assert.NotNull(accountOne);
        Assert.NotNull(accountTwo);

        var closingRequest = new CreateMonthlyClosingRequest(
            Year: year,
            Month: month,
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
            Year: 2035,
            Month: 1,
            AccountIds: [],
            Participants: ["Ana", "Bruno"]);

        var response = await _client.PostAsJsonAsync("/closing", closingRequest);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateClosing_WithAutoIncludedAccountsAndNoManualSelection_ShouldSucceedAndMarkPaid()
    {
        var (year, month) = BuildUniquePeriod();

        var createResponse = await _client.PostAsJsonAsync(
            "/api/accounts",
            new CreateAccountRequest(
                Name: "Rent",
                Amount: 1000m,
                DueDate: new DateTime(year, month, 10, 0, 0, 0, DateTimeKind.Utc),
                ParticipatesInDivision: true));

        createResponse.EnsureSuccessStatusCode();

        var closingRequest = new CreateMonthlyClosingRequest(
            Year: year,
            Month: month,
            AccountIds: [],
            Participants: ["Ana", "Bruno"]);

        var response = await _client.PostAsJsonAsync("/closing", closingRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<MonthlyClosingResponse>();
        Assert.NotNull(payload);
        Assert.Equal(1, payload!.AccountCount);

        var accounts = await _client.GetFromJsonAsync<List<AccountResponse>>($"/api/accounts?year={year}&month={month}");
        Assert.NotNull(accounts);
        Assert.DoesNotContain(accounts!, account => account.Name == "Rent" && account.Paid is false);
    }

    [Fact]
    public async Task ReopenClosing_ShouldUnpayAccountsFromSelectedClosing()
    {
        var (year, month) = BuildUniquePeriod();

        var first = await _client.PostAsJsonAsync(
            "/api/accounts",
            new CreateAccountRequest(
                Name: "Rent",
                Amount: 1000m,
                DueDate: new DateTime(year, month, 10, 0, 0, 0, DateTimeKind.Utc),
                ParticipatesInDivision: true));
        var second = await _client.PostAsJsonAsync(
            "/api/accounts",
            new CreateAccountRequest(
                Name: "Internet",
                Amount: 500m,
                DueDate: new DateTime(year, month, 15, 0, 0, 0, DateTimeKind.Utc),
                ParticipatesInDivision: false));

        var accountOne = await first.Content.ReadFromJsonAsync<AccountResponse>();
        var accountTwo = await second.Content.ReadFromJsonAsync<AccountResponse>();

        Assert.NotNull(accountOne);
        Assert.NotNull(accountTwo);

        var closeResponse = await _client.PostAsJsonAsync(
            "/closing",
            new CreateMonthlyClosingRequest(
                Year: year,
                Month: month,
                AccountIds: [accountTwo!.Id],
                Participants: ["Ana", "Bruno"]));

        closeResponse.EnsureSuccessStatusCode();

        var reopenResponse = await _client.PostAsJsonAsync(
            "/closing/reopen",
            new ReopenMonthlyClosingRequest(year, month));

        Assert.Equal(HttpStatusCode.OK, reopenResponse.StatusCode);

        var payload = await reopenResponse.Content.ReadFromJsonAsync<MonthlyClosingResponse>();
        Assert.NotNull(payload);
        Assert.True(payload!.IsReopened);
        Assert.NotNull(payload.ReopenedAtUtc);

        var unpaidAccounts = await _client.GetFromJsonAsync<List<AccountResponse>>($"/api/accounts?year={year}&month={month}");
        Assert.NotNull(unpaidAccounts);
        Assert.Contains(unpaidAccounts!, account => account.Id == accountOne!.Id && !account.Paid);
        Assert.Contains(unpaidAccounts!, account => account.Id == accountTwo!.Id && !account.Paid);
    }

    [Fact]
    public async Task CreateClosing_WithPaidAccountFromMonth_ShouldStillIncludeIt()
    {
        var (year, month) = BuildUniquePeriod();

        var createResponse = await _client.PostAsJsonAsync(
            "/api/accounts",
            new CreateAccountRequest(
                Name: "Security",
                Amount: 150m,
                DueDate: new DateTime(year, month, 23, 0, 0, 0, DateTimeKind.Utc)));

        createResponse.EnsureSuccessStatusCode();

        var created = await createResponse.Content.ReadFromJsonAsync<AccountResponse>();
        Assert.NotNull(created);

        var payResponse = await _client.PatchAsync($"/api/accounts/{created!.Id}/pay", null);
        payResponse.EnsureSuccessStatusCode();

        var closeResponse = await _client.PostAsJsonAsync(
            "/closing",
            new CreateMonthlyClosingRequest(
                Year: year,
                Month: month,
                AccountIds: [created.Id],
                Participants: ["Ana", "Bruno"]));

        Assert.Equal(HttpStatusCode.OK, closeResponse.StatusCode);

        var payload = await closeResponse.Content.ReadFromJsonAsync<MonthlyClosingResponse>();
        Assert.NotNull(payload);
        Assert.Equal(150m, payload!.TotalAmount);
        Assert.Equal(1, payload.AccountCount);
    }

    [Fact]
    public async Task GetClosing_WhenMonthIsClosed_ShouldReturnCurrentClosing()
    {
        var (year, month) = BuildUniquePeriod();

        var createResponse = await _client.PostAsJsonAsync(
            "/api/accounts",
            new CreateAccountRequest(
                Name: "Rent",
                Amount: 1000m,
                DueDate: new DateTime(year, month, 10, 0, 0, 0, DateTimeKind.Utc),
                ParticipatesInDivision: true));

        createResponse.EnsureSuccessStatusCode();

        var closeResponse = await _client.PostAsJsonAsync(
            "/closing",
            new CreateMonthlyClosingRequest(
                Year: year,
                Month: month,
                AccountIds: [],
                Participants: ["Ana", "Bruno"]));

        closeResponse.EnsureSuccessStatusCode();

        var getResponse = await _client.GetAsync($"/closing?year={year}&month={month}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var payload = await getResponse.Content.ReadFromJsonAsync<MonthlyClosingResponse>();
        Assert.NotNull(payload);
        Assert.Equal(2, payload!.ParticipantCount);
        Assert.Equal(["Ana", "Bruno"], payload.Participants);
    }

    [Fact]
    public async Task CreateClosing_WhenMonthAlreadyClosed_ShouldReturnBadRequest()
    {
        var (year, month) = BuildUniquePeriod();

        var createResponse = await _client.PostAsJsonAsync(
            "/api/accounts",
            new CreateAccountRequest(
                Name: "Rent",
                Amount: 1000m,
                DueDate: new DateTime(year, month, 10, 0, 0, 0, DateTimeKind.Utc),
                ParticipatesInDivision: true));

        createResponse.EnsureSuccessStatusCode();

        var firstClose = await _client.PostAsJsonAsync(
            "/closing",
            new CreateMonthlyClosingRequest(
                Year: year,
                Month: month,
                AccountIds: [],
                Participants: ["Ana", "Bruno"]));

        firstClose.EnsureSuccessStatusCode();

        var secondClose = await _client.PostAsJsonAsync(
            "/closing",
            new CreateMonthlyClosingRequest(
                Year: year,
                Month: month,
                AccountIds: [],
                Participants: ["Ana", "Bruno"]));

        Assert.Equal(HttpStatusCode.BadRequest, secondClose.StatusCode);
    }

    [Fact]
    public async Task RecloseAfterReopen_ShouldReuseSameClosingDocument()
    {
        var (year, month) = BuildUniquePeriod();

        var createResponse = await _client.PostAsJsonAsync(
            "/api/accounts",
            new CreateAccountRequest(
                Name: "Rent",
                Amount: 1000m,
                DueDate: new DateTime(year, month, 10, 0, 0, 0, DateTimeKind.Utc),
                ParticipatesInDivision: true));

        createResponse.EnsureSuccessStatusCode();

        var firstCloseResponse = await _client.PostAsJsonAsync(
            "/closing",
            new CreateMonthlyClosingRequest(
                Year: year,
                Month: month,
                AccountIds: [],
                Participants: ["Ana", "Bruno"]));

        firstCloseResponse.EnsureSuccessStatusCode();
        var firstClosing = await firstCloseResponse.Content.ReadFromJsonAsync<MonthlyClosingResponse>();
        Assert.NotNull(firstClosing);

        var reopenResponse = await _client.PostAsJsonAsync(
            "/closing/reopen",
            new ReopenMonthlyClosingRequest(year, month));

        reopenResponse.EnsureSuccessStatusCode();

        var secondCloseResponse = await _client.PostAsJsonAsync(
            "/closing",
            new CreateMonthlyClosingRequest(
                Year: year,
                Month: month,
                AccountIds: [],
                Participants: ["Ana", "Bruno"]));

        secondCloseResponse.EnsureSuccessStatusCode();
        var secondClosing = await secondCloseResponse.Content.ReadFromJsonAsync<MonthlyClosingResponse>();
        Assert.NotNull(secondClosing);

        Assert.Equal(firstClosing!.Id, secondClosing!.Id);
    }
}
