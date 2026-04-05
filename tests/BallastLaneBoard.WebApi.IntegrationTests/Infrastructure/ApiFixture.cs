using System.Net.Http.Json;
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
    /// Fetches a real JWT from Keycloak for <paramref name="username"/> and
    /// returns an HTTP client pre-configured with that bearer token.
    /// </summary>
    public async Task<HttpClient> CreateAuthenticatedClientAsync(
        string username, string password, CancellationToken ct = default)
    {
        var token  = await KeycloakFixture.Instance.GetTokenAsync(username, password, ct);
        var client = _factory!.CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);
        return client;
    }
}
