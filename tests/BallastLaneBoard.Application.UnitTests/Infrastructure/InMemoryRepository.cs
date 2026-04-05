using BallastLaneBoard.Application.Abstractions;
using BallastLaneBoard.Domain.Core;

namespace BallastLaneBoard.Application.UnitTests.Infrastructure;

public class InMemoryRepository<TEntity> : IRepository<TEntity>
    where TEntity : IEntity<Guid>
{
    protected readonly List<TEntity> _items = [];

    public IReadOnlyList<TEntity> Items => _items;

    public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _items.Add(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var index = _items.FindIndex(x => x.Id.Equals(entity.Id));
        if (index >= 0)
            _items[index] = entity;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _items.Remove(entity);
        return Task.CompletedTask;
    }

    public Task<TEntity?> FindAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_items.FirstOrDefault(x => x.Id.Equals(id)));
}
