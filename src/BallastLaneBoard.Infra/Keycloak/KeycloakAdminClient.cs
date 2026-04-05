using System.Net.Http.Json;
using System.Text.Json;
using BallastLaneBoard.Application.Identity;
using Microsoft.Extensions.Configuration;

namespace BallastLaneBoard.Infra.Keycloak;

internal sealed class KeycloakAdminClient(HttpClient httpClient, IConfiguration configuration) : IIdentityProviderClient
{
    public async Task<string> CreateUserAsync(string username, string email, string password, CancellationToken cancellationToken)
    {
        var baseUrl = configuration["IdentityProvider:AdminUrl"]
            ?? throw new InvalidOperationException("IdentityProvider:AdminUrl configuration is missing.");
        var realm = configuration["IdentityProvider:Realm"] ?? "ballast-lane-board";

        await EnsureAdminTokenAsync(baseUrl, cancellationToken);

        var payload = new
        {
            username,
            email,
            enabled = true,
            emailVerified = true,
            requiredActions = Array.Empty<string>(),
            credentials = new[]
            {
                new { type = "password", value = password, temporary = false }
            },
            realmRoles = new[] { "user" }
        };

        var response = await httpClient.PostAsJsonAsync(
            $"{baseUrl}/admin/realms/{realm}/users",
            payload,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var location = response.Headers.Location?.ToString()
            ?? throw new InvalidOperationException("Identity provider did not return a Location header.");

        // Location header format: .../users/{id}
        return location.Split('/').Last();
    }

    private async Task EnsureAdminTokenAsync(string baseUrl, CancellationToken cancellationToken)
    {
        if (httpClient.DefaultRequestHeaders.Authorization is not null)
            return;

        var adminUser = configuration["IdentityProvider:AdminUser"] ?? "admin";
        var adminPassword = configuration["IdentityProvider:AdminPassword"] ?? "admin";

        var tokenRequest = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = "admin-cli",
            ["username"] = adminUser,
            ["password"] = adminPassword
        });

        var tokenResponse = await httpClient.PostAsync(
            $"{baseUrl}/realms/master/protocol/openid-connect/token",
            tokenRequest,
            cancellationToken);

        tokenResponse.EnsureSuccessStatusCode();

        using var doc = await JsonDocument.ParseAsync(
            await tokenResponse.Content.ReadAsStreamAsync(cancellationToken),
            cancellationToken: cancellationToken);

        var accessToken = doc.RootElement.GetProperty("access_token").GetString()!;

        httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
    }
}
