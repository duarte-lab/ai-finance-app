using System.Net;
using System.Net.Http.Json;
using Application.People.DTOs;
using Xunit;

namespace API.IntegrationTests;

public class PeopleEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public PeopleEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateApiClient();
    }

    [Fact]
    public async Task CreatePerson_ThenGet_ShouldReturnCreatedPerson()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/people", new CreatePersonRequest("Carla"));

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<PersonResponse>();
        Assert.NotNull(created);
        Assert.Equal("Carla", created!.Name);
        Assert.Equal(DateTimeKind.Utc, created.CreatedAtUtc.Kind);

        var listResponse = await _client.GetAsync("/api/people");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        var list = await listResponse.Content.ReadFromJsonAsync<List<PersonResponse>>();
        Assert.NotNull(list);
        Assert.Contains(list!, item => item.Id == created.Id);
    }
}