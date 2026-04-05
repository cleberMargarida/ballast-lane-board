using BallastLaneBoard.Application.TaskManagement;
using BallastLaneBoard.Domain.TaskManagement;

namespace BallastLaneBoard.Application.UnitTests.Infrastructure;

public sealed class InMemoryTaskRepository : InMemoryRepository<TaskItem>, ITaskRepository
{
    public Task<List<TaskItem>> GetAllOrderedAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_items.OrderByDescending(t => t.CreatedAt).ToList());

    public Task<List<TaskItem>> GetByUserIdOrderedAsync(Guid userId, CancellationToken cancellationToken = default)
        => Task.FromResult(_items.Where(t => t.UserId == userId).OrderByDescending(t => t.CreatedAt).ToList());
}

public sealed class InMemoryTaskUoW : ITaskUoW
{
    private readonly InMemoryTaskRepository _tasks = new();

    public ITaskRepository Tasks => _tasks;

    public int CommitCount { get; private set; }

    public Task Commit(CancellationToken cancellationToken)
    {
        CommitCount++;
        return Task.CompletedTask;
    }

    public InMemoryTaskRepository TasksStore => _tasks;
}
