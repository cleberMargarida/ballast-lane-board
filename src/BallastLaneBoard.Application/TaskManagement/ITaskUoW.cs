using BallastLaneBoard.Application.Abstractions;

namespace BallastLaneBoard.Application.TaskManagement;

public interface ITaskUoW : IUnitOfWork
{
    ITaskRepository Tasks { get; }
}
