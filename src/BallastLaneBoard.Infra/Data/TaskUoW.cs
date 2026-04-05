using BallastLaneBoard.Application.TaskManagement;
using Npgsql;

namespace BallastLaneBoard.Infra.Data;

internal sealed class TaskUoW : DbConnectionUoW, ITaskUoW
{
    public TaskUoW(NpgsqlDataSource dataSource) : base(dataSource)
    {
        Tasks = new TaskRepository(Connection, Transaction);
    }

    public ITaskRepository Tasks { get; }
}
