using BallastLaneBoard.Domain.Core;

namespace BallastLaneBoard.Application.Abstractions;

/// <summary>
/// Represents a unit of work that encapsulates a set of changes to be committed atomically.
/// </summary>
public interface IUnitOfWork
{
    Task Commit() => Commit(CancellationToken.None);

    Task Commit(CancellationToken cancellationToken);
}

/// <summary>
/// Generic repository providing basic data access operations for entities identified by a GUID.
/// </summary>
public interface IRepository<TEntity>
    where TEntity : IEntity<Guid>
{
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task<TEntity?> FindAsync(Guid id, CancellationToken cancellationToken = default);
}
