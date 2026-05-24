using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Accounts.DTOs;
using Application.Notifications.DTOs;
using Xunit;

namespace API.IntegrationTests;

public class NotificationsEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public NotificationsEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateApiClient();
    }

    [Fact]
    public async Task GetDueNotifications_ShouldReturnAccountsDueTodayAndInThreeDays()
    {
        var todayUtc = DateTime.UtcNow.Date;

        await _client.PostAsJsonAsync(
            "/api/accounts",
            new CreateAccountRequest("Today bill", 120m, todayUtc));

        await _client.PostAsJsonAsync(
            "/api/accounts",
            new CreateAccountRequest("Three days bill", 240m, todayUtc.AddDays(3)));

        await _client.PostAsJsonAsync(
            "/api/accounts",
            new CreateAccountRequest("Five days bill", 300m, todayUtc.AddDays(5)));

        var response = await _client.GetAsync("/api/notifications/due");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        serializerOptions.Converters.Add(new JsonStringEnumConverter());

        var payload = await response.Content.ReadFromJsonAsync<List<NotificationResponse>>(serializerOptions);
        Assert.NotNull(payload);
        Assert.Equal(2, payload!.Count);
        Assert.Contains(payload, x => x.Type == NotificationType.DueToday && x.AccountName == "Today bill");
        Assert.Contains(payload, x => x.Type == NotificationType.DueInThreeDays && x.AccountName == "Three days bill");
    }
}