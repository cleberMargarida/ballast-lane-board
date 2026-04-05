using Testcontainers.Keycloak;

namespace BallastLaneBoard.WebApi.IntegrationTests.Infrastructure;

/// <summary>
/// Starts a Keycloak container with the test realm pre-imported and exposes
/// all OpenIdConnect / IdentityProvider settings via environment variables.
/// Variables are cleared on dispose so nothing leaks to other fixtures.
/// </summary>
public sealed class KeycloakFixture : IAsyncLifetime
{
    /// <summary>Signals that all Keycloak env vars are set and ready.</summary>
    internal static readonly TaskCompletionSource Ready = new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>The singleton instance, set during <see cref="InitializeAsync"/>.</summary>
    internal static KeycloakFixture Instance { get; private set; } = null!;

    internal const string Realm = "ballast-lane-board";
    private const string AdminUser = "admin";
    private const string AdminPass = "admin";

    private static readonly string[] EnvVarNames =
    [
        "OpenIdConnect__Authority",
        "OpenIdConnect__PublicAuthority",
        "OpenIdConnect__Audience",
        "OpenIdConnect__RoleClaimPath",
        "OpenIdConnect__RequireHttpsMetadata",
        "IdentityProvider__AdminUrl",
        "IdentityProvider__Realm",
        "IdentityProvider__AdminUser",
        "IdentityProvider__AdminPassword",
    ];

    private readonly KeycloakContainer _container = new KeycloakBuilder()
        .WithImage("quay.io/keycloak/keycloak:26.2")
        .WithUsername(AdminUser)
        .WithPassword(AdminPass)
        // WithResourceMapping(FileInfo, string) treats the string as a target DIRECTORY.
        // Keycloak 26 requires the file name to be "{realm}-realm.json".
        .WithResourceMapping(
            new FileInfo(Path.Combine(AppContext.BaseDirectory, "Infrastructure", "ballast-lane-board-realm.json")),
            "/opt/keycloak/data/import/")
        // KeycloakBuilder.Init() already adds "start-dev"; we only append the extra flag.
        .WithCommand("--import-realm")
        .Build();

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();
        Instance = this;

        var baseAddress = _container.GetBaseAddress(); // e.g. "http://localhost:PORT/"
        var authority   = $"{baseAddress}realms/{Realm}";
        var adminUrl    = baseAddress.TrimEnd('/');

        Environment.SetEnvironmentVariable("OpenIdConnect__Authority",            authority);
        Environment.SetEnvironmentVariable("OpenIdConnect__PublicAuthority",      authority);
        Environment.SetEnvironmentVariable("OpenIdConnect__Audience",             "ballast-lane-board-api");
        Environment.SetEnvironmentVariable("OpenIdConnect__RoleClaimPath",        "realm_access.roles");
        Environment.SetEnvironmentVariable("OpenIdConnect__RequireHttpsMetadata", "false");
        Environment.SetEnvironmentVariable("IdentityProvider__AdminUrl",          adminUrl);
        Environment.SetEnvironmentVariable("IdentityProvider__Realm",            Realm);
        Environment.SetEnvironmentVariable("IdentityProvider__AdminUser",        AdminUser);
        Environment.SetEnvironmentVariable("IdentityProvider__AdminPassword",    AdminPass);
        Ready.TrySetResult();
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var name in EnvVarNames)
            Environment.SetEnvironmentVariable(name, null);

        await _container.DisposeAsync();
    }
}
