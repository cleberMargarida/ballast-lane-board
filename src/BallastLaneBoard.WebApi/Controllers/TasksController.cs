using BallastLaneBoard.Application.TaskManagement;
using BallastLaneBoard.Application.TaskManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BallastLaneBoard.WebApi.Controllers;

/// <summary>
/// Task CRUD operations. All endpoints require authentication.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController(TaskService taskService) : ApiControllerBase
{
    /// <summary>
    /// Lists all tasks for the current user. Admins see all tasks.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        return Ok(await taskService.GetAll(User.UserId, User.IsAdmin, ct));
    }

    /// <summary>
    /// Gets a specific task by ID.
    /// </summary>
    [HttpGet("{taskId:guid}")]
    public async Task<IActionResult> GetById(Guid taskId, CancellationToken ct)
    {
        var result = await taskService.GetById(taskId, User.UserId, User.IsAdmin, ct);
        return OkOrError(result);
    }

    /// <summary>
    /// Creates a new task for the authenticated user.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create(CreateTaskRequest request, CancellationToken ct)
    {
        var result = await taskService.Create(request, User.UserId, ct);
        return CreatedAtOrError(result, nameof(GetById), t => new { TaskId = t.Id });
    }

    /// <summary>
    /// Updates an existing task.
    /// </summary>
    [HttpPut("{taskId:guid}")]
    public async Task<IActionResult> Update(Guid taskId, UpdateTaskRequest request, CancellationToken ct)
    {
        var result = await taskService.Update(taskId, request, User.UserId, ct);
        return OkOrError(result);
    }

    /// <summary>
    /// Changes the status of a task.
    /// </summary>
    [HttpPatch("{taskId:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid taskId, ChangeTaskStatusRequest request, CancellationToken ct)
    {
        var result = await taskService.ChangeStatus(taskId, request.Status, User.UserId, ct);
        return NoContentOrError(result);
    }

    /// <summary>
    /// Deletes a task.
    /// </summary>
    [HttpDelete("{taskId:guid}")]
    public async Task<IActionResult> Delete(Guid taskId, CancellationToken ct)
    {
        var result = await taskService.Delete(taskId, User.UserId, ct);
        return NoContentOrError(result);
    }
}
