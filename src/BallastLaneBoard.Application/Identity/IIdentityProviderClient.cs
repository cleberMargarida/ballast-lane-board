namespace BallastLaneBoard.Application.Identity;

/// <summary>
/// Abstraction over an external identity provider's Admin API for user provisioning.
/// </summary>
public interface IIdentityProviderClient
{
    /// <summary>
    /// Creates a user in the identity provider and returns the external subject (user ID).
    /// </summary>
    Task<string> CreateUserAsync(string username, string email, string password, CancellationToken cancellationToken);
}
