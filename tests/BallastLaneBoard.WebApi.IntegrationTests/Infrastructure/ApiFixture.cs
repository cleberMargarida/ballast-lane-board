using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BallastLaneBoard.Application.Identity.Models;

namespace BallastLaneBoard.WebApi.IntegrationTests.Infrastructure;

/// <summary>
/// Builds the <see cref="ApiFactory"/> and seeds the two test users in both
/// Keycloak (via the register endpoint) and the application database.
/// Waits for <see cref="PostgresFixture"/> and <see cref="KeycloakFixture"/>
/// readiness signals before starting.
/// </summary>
public sealed class ApiFixture : IAsyncLifetime
{
    // ── Test-user constants ───────────────────────────────────────────────
    internal const string User1Name     = "testuser1";
    internal const string User1Email    = "testuser1@test.local";
    internal const string User1Password = "TestPass1!";

    internal const string User2Name     = "testuser2";
    internal const string User2Email    = "testuser2@test.local";
    internal const string User2Password = "TestPass2!";

    internal const string AdminName     = "admin";
    internal const string AdminPassword = "admin";

    private ApiFactory? _factory;

    // ── Lifecycle ─────────────────────────────────────────────────────────

    public async ValueTask InitializeAsync()
    {
        // Wait for both container fixtures to be fully initialized and env vars set.
        await PostgresFixture.Ready.Task;
        await KeycloakFixture.Ready.Task;

        _factory = new ApiFactory();

        // Register both test users so they exist in Keycloak AND the app DB.
        var http = _factory.CreateClient();
        var r1 = await http.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest(User1Name, User1Email, User1Password));
        if (!r1.IsSuccessStatusCode)
            throw new InvalidOperationException(
                $"Register user1 failed ({r1.StatusCode}): {await r1.Content.ReadAsStringAsync()}");
        var r2 = await http.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest(User2Name, User2Email, User2Password));
        if (!r2.IsSuccessStatusCode)
            throw new InvalidOperationException(
                $"Register user2 failed ({r2.StatusCode}): {await r2.Content.ReadAsStringAsync()}");
    }

    public async ValueTask DisposeAsync()
    {
        if (_factory is not null)
            await _factory.DisposeAsync();
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    /// <summary>Creates an unauthenticated HTTP client against the test API.</summary>
    public HttpClient CreateClient() => _factory!.CreateClient();

    /// <summary>
    /// Fetches a real JWT from Keycloak (using <c>OpenIdConnect__Authority</c> from the
    /// environment) and returns an HTTP client that injects the bearer token via a
    /// <see cref="DelegatingHandler"/> — no direct dependency on
    /// <see cref="KeycloakFixture.Instance"/>.
    /// </summary>
    public async Task<HttpClient> CreateAuthenticatedClientAsync(
        string username, string password, CancellationToken ct = default)
    {
        var authority     = Environment.GetEnvironmentVariable("OpenIdConnect__Authority")!;
        var tokenEndpoint = $"{authority}/protocol/openid-connect/token";

        using var tokenHttp = new HttpClient();
        var response = await tokenHttp.PostAsync(tokenEndpoint,
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"]  = "test-client",
                ["username"]   = username,
                ["password"]   = password,
            }), ct);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"Keycloak token request failed ({response.StatusCode}): {await response.Content.ReadAsStringAsync(ct)}");

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        var token = doc.RootElement.GetProperty("access_token").GetString()!;

        return _factory!.CreateDefaultClient(new BearerTokenHandler(token));
    }

    private sealed class BearerTokenHandler(string token) : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return base.SendAsync(request, cancellationToken);
        }
    }
}
