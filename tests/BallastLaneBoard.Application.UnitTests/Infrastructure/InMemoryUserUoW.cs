using BallastLaneBoard.Application.Identity;
using BallastLaneBoard.Domain.Identity;

namespace BallastLaneBoard.Application.UnitTests.Infrastructure;

public sealed class InMemoryUserRepository : InMemoryRepository<AppUser>, IUserRepository
{
    public Task<AppUser?> FindBySubjectAsync(string externalSubject, CancellationToken cancellationToken = default)
        => Task.FromResult(_items.FirstOrDefault(u => u.ExternalSubject == externalSubject));
}

public sealed class InMemoryUserUoW : IUserUoW
{
    private readonly InMemoryUserRepository _users = new();

    public IUserRepository Users => _users;

    public int CommitCount { get; private set; }

    public Task Commit(CancellationToken cancellationToken)
    {
        CommitCount++;
        return Task.CompletedTask;
    }

    public InMemoryUserRepository UsersStore => _users;
}
