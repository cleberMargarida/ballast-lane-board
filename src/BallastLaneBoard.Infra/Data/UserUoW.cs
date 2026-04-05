using BallastLaneBoard.Application.Identity;
using Npgsql;

namespace BallastLaneBoard.Infra.Data;

internal sealed class UserUoW(NpgsqlDataSource dataSource) : DbConnectionUoW(dataSource), IUserUoW
{
    private UserRepository? _users;
    public IUserRepository Users => _users ??= new UserRepository(GetContextAsync());
}
