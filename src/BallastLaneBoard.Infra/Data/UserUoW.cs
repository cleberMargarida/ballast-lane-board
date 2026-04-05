using BallastLaneBoard.Application.Identity;
using Npgsql;

namespace BallastLaneBoard.Infra.Data;

internal sealed class UserUoW : DbConnectionUoW, IUserUoW
{
    public UserUoW(NpgsqlDataSource dataSource) : base(dataSource)
    {
        Users = new UserRepository(Connection, Transaction);
    }

    public IUserRepository Users { get; }
}
