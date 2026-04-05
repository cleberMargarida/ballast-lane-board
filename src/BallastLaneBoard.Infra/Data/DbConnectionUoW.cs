using BallastLaneBoard.Application.Abstractions;
using Npgsql;

namespace BallastLaneBoard.Infra.Data;

/// <summary>
/// Base Unit of Work backed by a lazily-opened NpgsqlConnection + NpgsqlTransaction.
/// The connection and transaction are opened asynchronously on first repository access.
/// </summary>
internal abstract class DbConnectionUoW(NpgsqlDataSource dataSource) : IUnitOfWork, IAsyncDisposable
{
    private readonly Task<(NpgsqlConnection Connection, NpgsqlTransaction Transaction)> _initTask = InitAsync(dataSource);
    private bool _committed;

    private static async Task<(NpgsqlConnection Connection, NpgsqlTransaction Transaction)> InitAsync(NpgsqlDataSource dataSource)
    {
        var connection = dataSource.CreateConnection();
        await connection.OpenAsync();
        var transaction = await connection.BeginTransactionAsync();
        return (connection, transaction);
    }

    protected Task<(NpgsqlConnection Connection, NpgsqlTransaction Transaction)> GetContextAsync() => _initTask;

    public async Task Commit(CancellationToken cancellationToken)
    {
        var (_, transaction) = await _initTask;
        await transaction.CommitAsync(cancellationToken);
        _committed = true;
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            var (connection, transaction) = await _initTask;

            if (!_committed)
            {
                try { await transaction.RollbackAsync(); }
                catch { /* already disposed or connection dropped */ }
            }

            await transaction.DisposeAsync();
            await connection.DisposeAsync();
        }
        catch { /* init failed — no resources to free */ }
    }
}
