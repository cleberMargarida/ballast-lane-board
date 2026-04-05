using BallastLaneBoard.Application.Identity;

namespace BallastLaneBoard.Application.UnitTests.Infrastructure;

public sealed class FakeIdentityProviderClient : IIdentityProviderClient
{
    public string NextSubject { get; set; } = Guid.NewGuid().ToString();

    public Task<string> CreateUserAsync(string username, string email, string password, CancellationToken cancellationToken)
        => Task.FromResult(NextSubject);
}
