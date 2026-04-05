using BallastLaneBoard.Application.Abstractions;
using BallastLaneBoard.Application.Identity;
using BallastLaneBoard.Application.Identity.Models;
using BallastLaneBoard.Application.UnitTests.Infrastructure;

namespace BallastLaneBoard.Application.UnitTests;

public class UserServiceTests
{
    private readonly InMemoryUserUoW _uow = new();
    private readonly FakeIdentityProviderClient _identityProvider = new();
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _sut = new UserService(_uow, _identityProvider);
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsSuccessAndCommits()
    {
        _identityProvider.NextSubject = "sub-abc";
        var request = new RegisterRequest("testuser", "test@example.com", "Password1!");

        var result = await _sut.Register(request, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("testuser", result.Value!.Username);
        Assert.Equal("test@example.com", result.Value.Email);
        Assert.Single(_uow.UsersStore.Items);
        Assert.Equal(1, _uow.CommitCount);
    }

    [Fact]
    public async Task Register_WithEmptyUsername_ReturnsFailure()
    {
        var request = new RegisterRequest("", "test@example.com", "Password1!");

        var result = await _sut.Register(request, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Empty(_uow.UsersStore.Items);
        Assert.Equal(0, _uow.CommitCount);
    }

    [Fact]
    public async Task GetBySubject_ExistingUser_ReturnsSuccess()
    {
        _identityProvider.NextSubject = "sub-abc";
        await _sut.Register(new RegisterRequest("testuser", "test@example.com", "pw"), CancellationToken.None);

        var result = await _sut.GetBySubject("sub-abc", CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("testuser", result.Value!.Username);
    }

    [Fact]
    public async Task GetBySubject_NonExistentUser_ReturnsNotFound()
    {
        var result = await _sut.GetBySubject("nonexistent", CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Equal(ApplicationError.NotFound.GetHashCode(), result.Error);
    }

    [Fact]
    public async Task Sync_ExistingUser_UpdatesLastSeen()
    {
        _identityProvider.NextSubject = "sub-abc";
        await _sut.Register(new RegisterRequest("testuser", "test@example.com", "pw"), CancellationToken.None);
        var commitsBefore = _uow.CommitCount;

        var result = await _sut.Sync("sub-abc", CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(commitsBefore + 1, _uow.CommitCount);
    }

    [Fact]
    public async Task Sync_NonExistentUser_ReturnsNotFound()
    {
        var result = await _sut.Sync("nonexistent", CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Equal(ApplicationError.NotFound.GetHashCode(), result.Error);
    }
}
