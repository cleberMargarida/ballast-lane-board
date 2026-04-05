using Testcontainers.PostgreSql;

namespace BallastLaneBoard.WebApi.IntegrationTests.Infrastructure;

/// <summary>
/// Starts a PostgreSQL container and exposes its connection string via the
/// <c>ConnectionStrings__DefaultConnection</c> environment variable.
/// The variable is cleared on dispose so nothing leaks to other fixtures.
/// </summary>
public sealed class PostgresFixture : IAsyncLifetime
{
    /// <summary>Signals that the Postgres env var is set and ready.</summary>
    internal static readonly TaskCompletionSource Ready = new(TaskCreationOptions.RunContinuationsAsynchronously);

    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .Build();

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();
        Environment.SetEnvironmentVariable(
            "ConnectionStrings__DefaultConnection",
            _container.GetConnectionString());
        Ready.TrySetResult();
    }

    public async ValueTask DisposeAsync()
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", null);
        await _container.DisposeAsync();
    }
}
