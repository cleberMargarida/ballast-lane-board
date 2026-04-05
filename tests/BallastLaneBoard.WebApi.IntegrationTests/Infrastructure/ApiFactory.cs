using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BallastLaneBoard.Application.Identity.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace BallastLaneBoard.WebApi.IntegrationTests.Infrastructure;

/// <summary>
/// <see cref="WebApplicationFactory{TEntryPoint}"/> configured for integration tests.
/// Implements <see cref="IAsyncLifetime"/> to seed the two test users in both
/// Keycloak (via the register endpoint) and the application database.
/// Waits for <see cref="PostgresFixture"/> and <see cref="KeycloakFixture"/>
/// readiness signals before starting.
/// <c>CreateClient()</c> and <c>DisposeAsync()</c> are provided by the base class.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
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

    // ── WebApplicationFactory ─────────────────────────────────────────────

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Re-add environment variables at the end of the config pipeline so they
        // take the highest precedence over any appsettings file values.
        builder.ConfigureAppConfiguration((_, config) => config.AddEnvironmentVariables());
    }

    // ── IAsyncLifetime ────────────────────────────────────────────────────
    // DisposeAsync is inherited from WebApplicationFactory<T> (IAsyncDisposable).

    public async ValueTask InitializeAsync()
    {
        // Wait for both container fixtures to be fully initialized and env vars set.
        await PostgresFixture.Ready.Task;
        await KeycloakFixture.Ready.Task;

        // Register both test users so they exist in Keycloak AND the app DB.
        var http = CreateClient();
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

    // ── Authenticated client ──────────────────────────────────────────────

    /// <summary>
    /// Fetches a real JWT from Keycloak (using <c>OpenIdConnect__Authority</c> from the
    /// environment) and returns an HTTP client that injects the bearer token via a
    /// <see cref="DelegatingHandler"/>.
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

        return CreateDefaultClient(new BearerTokenHandler(token));
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
