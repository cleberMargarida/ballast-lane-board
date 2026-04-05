using System.Net;
using System.Net.Http.Json;
using BallastLaneBoard.Application.Identity.Models;
using BallastLaneBoard.WebApi.IntegrationTests.Infrastructure;

namespace BallastLaneBoard.WebApi.IntegrationTests;

[Collection("Integration")]
public class AuthControllerTests(ApiFixture api)
{
    [Fact]
    public async Task Register_ValidRequest_Returns201()
    {
        var ct = TestContext.Current.CancellationToken;
        var client = api.CreateClient();
        var unique = Guid.NewGuid().ToString("N")[..8];
        var request = new RegisterRequest($"usr{unique}", $"usr{unique}@test.local", "Password1!");

        var response = await client.PostAsJsonAsync("/api/auth/register", request, ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Me_WithoutAuth_Returns401()
    {
        var ct = TestContext.Current.CancellationToken;
        var client = api.CreateClient();

        var response = await client.GetAsync("/api/auth/me", ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_Authenticated_ReturnsUser()
    {
        var ct = TestContext.Current.CancellationToken;
        // User1 was registered during ApiFixture.InitializeAsync with a real
        // Keycloak subject stored as ExternalSubject in the app DB.
        var client = await api.CreateAuthenticatedClientAsync(
            ApiFixture.User1Name, ApiFixture.User1Password, ct);

        var response = await client.GetAsync("/api/auth/me", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Sync_Authenticated_Returns200()
    {
        var ct = TestContext.Current.CancellationToken;
        var client = await api.CreateAuthenticatedClientAsync(
            ApiFixture.User1Name, ApiFixture.User1Password, ct);

        var response = await client.PostAsync("/api/auth/sync", null, ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Sync_WithoutAuth_Returns401()
    {
        var ct = TestContext.Current.CancellationToken;
        var client = api.CreateClient();

        var response = await client.PostAsync("/api/auth/sync", null, ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
