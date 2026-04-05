using BallastLaneBoard.Application.Identity;
using BallastLaneBoard.Application.Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BallastLaneBoard.WebApi.Controllers;

/// <summary>
/// Authentication and user management endpoints.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController(UserService userService) : ControllerBase
{
    /// <summary>
    /// Registers a new user via the identity provider and mirrors locally.
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken ct)
    {
        var result = await userService.Register(request, ct);
        return result.IsSuccess ? Created("", result.Value) : BadRequest(result.Error);
    }

    /// <summary>
    /// Returns the current authenticated user's profile.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var subject = User.Subject;
        var result = await userService.GetBySubject(subject, ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound();
    }

    /// <summary>
    /// Syncs the current user's last-seen timestamp.
    /// </summary>
    [HttpPost("sync")]
    [Authorize]
    public async Task<IActionResult> Sync(CancellationToken ct)
    {
        var result = await userService.Sync(User.Subject, ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound();
    }
}
