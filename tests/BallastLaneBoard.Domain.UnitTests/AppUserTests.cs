using BallastLaneBoard.Domain.Identity;

namespace BallastLaneBoard.Domain.UnitTests;

public class AppUserTests
{
    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        var result = AppUser.Create("sub-123", "testuser", "test@example.com", UserRole.User);

        Assert.True(result.IsSuccess);
        Assert.Equal("testuser", result.Value.Username);
        Assert.Equal("test@example.com", result.Value.Email);
        Assert.Equal(UserRole.User, result.Value.Role);
        Assert.Equal("sub-123", result.Value.ExternalSubject);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyUsername_ReturnsFailure(string? username)
    {
        var result = AppUser.Create("sub-123", username!, "test@example.com", UserRole.User);

        Assert.True(result.IsFailed);
        Assert.Equal(UserError.UsernameRequired.GetHashCode(), result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("invalidemail")]
    public void Create_WithInvalidEmail_ReturnsFailure(string? email)
    {
        var result = AppUser.Create("sub-123", "testuser", email!, UserRole.User);

        Assert.True(result.IsFailed);
    }

    [Fact]
    public void Create_NormalizesEmail()
    {
        var result = AppUser.Create("sub-123", "testuser", "Test@Example.COM", UserRole.User);

        Assert.True(result.IsSuccess);
        Assert.Equal("test@example.com", result.Value.Email);
    }

    [Fact]
    public void Sync_UpdatesLastSeenAt()
    {
        var user = CreateUser();
        var previousLastSeen = user.LastSeenAt;

        var result = user.Sync();

        Assert.True(result.IsSuccess);
        Assert.True(user.LastSeenAt >= previousLastSeen);
    }

    private static AppUser CreateUser()
    {
        var result = AppUser.Create("sub-123", "testuser", "test@example.com", UserRole.User);
        return result.Value;
    }
}
