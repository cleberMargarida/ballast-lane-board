using BallastLaneBoard.Domain.Core;

namespace BallastLaneBoard.Domain.Identity;

/// <summary>
/// Aggregate root representing an application user, mirrored from an external identity provider.
/// </summary>
public sealed class AppUser : IAggregateRoot
{
    private AppUser() { }

    public Guid Id { get; private init; }
    public string ExternalSubject { get; private init; } = null!;
    public string Username { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public UserRole Role { get; private set; }
    public DateTimeOffset CreatedAt { get; private init; }
    public DateTimeOffset LastSeenAt { get; private set; }

    /// <summary>
    /// Factory method — creates a local user mirror from identity provider registration.
    /// </summary>
    public static Result<AppUser> Create(
        string externalSubject,
        string username,
        string email,
        UserRole role)
    {
        if (string.IsNullOrWhiteSpace(username))
            return Result.Fail(UserError.UsernameRequired);

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            return Result.Fail(UserError.EmailRequired);

        var now = DateTimeOffset.UtcNow;

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            ExternalSubject = externalSubject,
            Username = username.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            Role = role,
            CreatedAt = now,
            LastSeenAt = now
        };

        return Result.Ok(user);
    }

    /// <summary>
    /// Updates last-seen timestamp when the user logs in or makes a request.
    /// </summary>
    public Result Sync()
    {
        LastSeenAt = DateTimeOffset.UtcNow;
        return Result.Ok();
    }

    /// <summary>
    /// Reconstitutes an AppUser from persisted data. Used only by the data-access layer.
    /// </summary>
    internal static AppUser Reconstitute(
        Guid id, string externalSubject, string username, string email,
        UserRole role, DateTimeOffset createdAt, DateTimeOffset lastSeenAt)
    {
        return new AppUser
        {
            Id = id,
            ExternalSubject = externalSubject,
            Username = username,
            Email = email,
            Role = role,
            CreatedAt = createdAt,
            LastSeenAt = lastSeenAt
        };
    }
}
