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
        _client = factory.CreateAuthenticatedApiClient();
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

    [Fact]
    public async Task DeletePerson_ShouldMarkAsDeleted_AndHideFromGet()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/people", new CreatePersonRequest("Pessoa Excluir"));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<PersonResponse>();
        Assert.NotNull(created);

        var deleteResponse = await _client.DeleteAsync($"/api/people/{created!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var listResponse = await _client.GetAsync("/api/people");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        var list = await listResponse.Content.ReadFromJsonAsync<List<PersonResponse>>();
        Assert.NotNull(list);
        Assert.DoesNotContain(list!, item => item.Id == created.Id);
    }

    [Fact]
    public async Task UpdatePerson_ShouldReturnUpdatedName()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/people", new CreatePersonRequest("Nome Antigo"));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<PersonResponse>();
        Assert.NotNull(created);

        var updateResponse = await _client.PutAsJsonAsync($"/api/people/{created!.Id}", new UpdatePersonRequest("Nome Novo"));
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updated = await updateResponse.Content.ReadFromJsonAsync<PersonResponse>();
        Assert.NotNull(updated);
        Assert.Equal("Nome Novo", updated!.Name);
        Assert.True((updated.CreatedAtUtc - created.CreatedAtUtc).Duration() < TimeSpan.FromSeconds(1));
    }
}