using BallastLaneBoard.Domain.Identity;

namespace BallastLaneBoard.Application.Identity.Models;

public sealed record RegisterRequest(string Username, string Email, string Password);

public sealed record UserResponse(
    Guid Id,
    string Username,
    string Email,
    UserRole Role,
    DateTimeOffset CreatedAt,
    DateTimeOffset LastSeenAt);
