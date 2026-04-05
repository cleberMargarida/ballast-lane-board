using BallastLaneBoard.Application.Abstractions;
using Npgsql;

namespace BallastLaneBoard.Infra.Data;

/// <summary>
/// Base Unit of Work backed by an NpgsqlConnection + NpgsqlTransaction.
/// Repositories execute SQL immediately within the transaction; Commit() commits it.
/// </summary>
internal abstract class DbConnectionUoW : IUnitOfWork, IAsyncDisposable
{
    private readonly NpgsqlConnection _connection;
    private readonly NpgsqlTransaction _transaction;
    private bool _committed;

    protected DbConnectionUoW(NpgsqlDataSource dataSource)
    {
        _connection = dataSource.CreateConnection();
        _connection.Open();
        _transaction = _connection.BeginTransaction();
    }

    protected NpgsqlConnection Connection => _connection;
    protected NpgsqlTransaction Transaction => _transaction;

    public async Task Commit(CancellationToken cancellationToken)
    {
        await _transaction.CommitAsync(cancellationToken);
        _committed = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (!_committed)
        {
            try { await _transaction.RollbackAsync(); }
            catch { /* already disposed or connection dropped */ }
        }

        await _transaction.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
