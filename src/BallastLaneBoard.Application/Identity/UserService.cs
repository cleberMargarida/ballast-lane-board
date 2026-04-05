using BallastLaneBoard.Application.Abstractions;
using BallastLaneBoard.Application.Identity.Models;
using BallastLaneBoard.Domain.Core;
using BallastLaneBoard.Domain.Identity;

namespace BallastLaneBoard.Application.Identity;

public class UserService(IUserUoW uow, IIdentityProviderClient identityProvider)
{
    /// <summary>
    /// Registers a new user via the identity provider, then mirrors locally.
    /// </summary>
    public async Task<Result<UserResponse>> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var subject = await identityProvider.CreateUserAsync(
            request.Username, request.Email, request.Password, cancellationToken);

        var result = AppUser.Create(
            subject,
            request.Username,
            request.Email,
            UserRole.User);

        if (result.IsFailed)
            return Result.Fail<UserResponse>(result.Error!.Value);

        var user = result.Value;

        await uow.Users.AddAsync(user, cancellationToken);

        await uow.Commit(cancellationToken);

        return Result.Ok(MapToResponse(user));
    }

    /// <summary>
    /// Gets the current user by external subject, or null if not found.
    /// </summary>
    public async Task<Result<UserResponse>> GetBySubject(
        string subject,
        CancellationToken cancellationToken)
    {
        var user = await uow.Users.FindBySubjectAsync(subject, cancellationToken);

        if (user is null)
            return Result.Fail(ApplicationError.NotFound);

        return Result.Ok(MapToResponse(user));
    }

    /// <summary>
    /// Syncs the current user's last-seen timestamp.
    /// </summary>
    public async Task<Result<UserResponse>> Sync(
        string subject,
        CancellationToken cancellationToken)
    {
        var user = await uow.Users.FindBySubjectAsync(subject, cancellationToken);

        if (user is null)
            return Result.Fail(ApplicationError.NotFound);

        var result = user.Sync();

        if (result.IsFailed)
            return Result.Fail<UserResponse>(result.Error!.Value);

        await uow.Users.UpdateAsync(user, cancellationToken);

        await uow.Commit(cancellationToken);

        return Result.Ok(MapToResponse(user));
    }

    private static UserResponse MapToResponse(AppUser user) =>
        new(user.Id, user.Username, user.Email, user.Role, user.CreatedAt, user.LastSeenAt);
}
