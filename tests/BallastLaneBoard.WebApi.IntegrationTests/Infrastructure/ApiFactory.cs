using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace BallastLaneBoard.WebApi.IntegrationTests.Infrastructure;

/// <summary>
/// <see cref="WebApplicationFactory{TEntryPoint}"/> configured for integration tests.
/// Configuration is read purely from environment variables set by <see cref="PostgresFixture"/>
/// and <see cref="KeycloakFixture"/> before the host is built.
/// </summary>
internal sealed class ApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Re-add environment variables at the end of the config pipeline so they
        // take the highest precedence over any appsettings file values.
        builder.ConfigureAppConfiguration((_, config) => config.AddEnvironmentVariables());
    }
}
