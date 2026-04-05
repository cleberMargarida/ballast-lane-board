using BallastLaneBoard.Application.Abstractions;
using BallastLaneBoard.Domain.TaskManagement;

namespace BallastLaneBoard.Application.TaskManagement;

public interface ITaskRepository : IRepository<TaskItem>
{
    Task<List<TaskItem>> GetAllOrderedAsync(CancellationToken cancellationToken = default);

    Task<List<TaskItem>> GetByUserIdOrderedAsync(Guid userId, CancellationToken cancellationToken = default);
}
