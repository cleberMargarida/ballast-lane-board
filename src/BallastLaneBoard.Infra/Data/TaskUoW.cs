using BallastLaneBoard.Application.TaskManagement;
using Npgsql;

namespace BallastLaneBoard.Infra.Data;

internal sealed class TaskUoW(NpgsqlDataSource dataSource) : DbConnectionUoW(dataSource), ITaskUoW
{
    private TaskRepository? _tasks;
    public ITaskRepository Tasks => _tasks ??= new TaskRepository(GetContextAsync());
}
