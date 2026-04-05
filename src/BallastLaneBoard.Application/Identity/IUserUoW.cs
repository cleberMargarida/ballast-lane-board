using BallastLaneBoard.Application.Abstractions;

namespace BallastLaneBoard.Application.Identity;

public interface IUserUoW : IUnitOfWork
{
    IUserRepository Users { get; }
}
