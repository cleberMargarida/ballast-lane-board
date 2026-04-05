using Microsoft.Extensions.Hosting;
using Npgsql;

namespace BallastLaneBoard.Infra;

/// <summary>
/// Applies the idempotent SQL init script on startup to ensure schema and seed data exist.
/// </summary>
internal sealed class DatabaseMigrationHostedService(NpgsqlDataSource dataSource) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var sql = GetInitSql();

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, connection);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static string GetInitSql()
    {
        var assembly = typeof(DatabaseMigrationHostedService).Assembly;
        using var stream = assembly.GetManifestResourceStream("BallastLaneBoard.Infra.Sql.init.sql")
            ?? throw new InvalidOperationException("Embedded resource 'Sql/init.sql' not found.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
