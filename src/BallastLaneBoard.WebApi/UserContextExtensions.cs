using System.Security.Claims;

namespace BallastLaneBoard.WebApi;

internal static class UserContextExtensions
{
    private const string SubjectClaim = "sub";
    private const string AppUserIdClaim = "app_user_id";
    private const string AdminRole = "admin";

    extension(ClaimsPrincipal user)
    {
        internal string Subject => user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue(SubjectClaim)
            ?? throw new UnauthorizedAccessException("Missing subject claim.");

        internal Guid UserId => ResolveUserId(user, user.Subject);

        internal bool IsAdmin => user.IsInRole(AdminRole);
    }

    private static Guid ResolveUserId(ClaimsPrincipal user, string subject)
    {
        var appUserIdClaim = user.FindFirstValue(AppUserIdClaim);
        if (appUserIdClaim is not null)
        {
            if (Guid.TryParse(appUserIdClaim, out var appUserId))
                return appUserId;

            throw new UnauthorizedAccessException($"Invalid {AppUserIdClaim} claim.");
        }

        if (Guid.TryParse(subject, out var subjectAsGuid))
            return subjectAsGuid;

        throw new UnauthorizedAccessException("Unable to resolve local user id from claims.");
    }
}