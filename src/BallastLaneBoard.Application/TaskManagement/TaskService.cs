using BallastLaneBoard.Application.Abstractions;
using BallastLaneBoard.Application.TaskManagement.Models;
using BallastLaneBoard.Domain.Core;
using BallastLaneBoard.Domain.TaskManagement;

namespace BallastLaneBoard.Application.TaskManagement;

public class TaskService(ITaskUoW uow)
{
    /// <summary>
    /// Creates a new task for the authenticated user.
    /// </summary>
    public async Task<Result<TaskResponse>> Create(
        CreateTaskRequest request,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var result = TaskItem.Create(request.Title, request.Description, request.DueDate, userId);

        if (result.IsFailed)
            return Result.Fail<TaskResponse>(result.Error!.Value);

        var task = result.Value;

        await uow.Tasks.AddAsync(task, cancellationToken);

        await uow.Commit(cancellationToken);

        return Result.Ok(MapToResponse(task));
    }

    /// <summary>
    /// Gets all tasks for the authenticated user. Admins see all tasks.
    /// </summary>
    public async Task<List<TaskResponse>> GetAll(Guid userId, bool isAdmin, CancellationToken cancellationToken)
    {
        var tasks = isAdmin
            ? await uow.Tasks.GetAllOrderedAsync(cancellationToken)
            : await uow.Tasks.GetByUserIdOrderedAsync(userId, cancellationToken);

        return tasks.Select(MapToResponse).ToList();
    }

    /// <summary>
    /// Gets a task by ID. Enforces ownership unless admin.
    /// </summary>
    public async Task<Result<TaskResponse>> GetById(
        Guid taskId,
        Guid userId,
        bool isAdmin,
        CancellationToken cancellationToken)
    {
        var task = await uow.Tasks.FindAsync(taskId, cancellationToken);

        if (task is null)
            return Result.Fail(ApplicationError.NotFound);

        if (!isAdmin && task.UserId != userId)
            return Result.Fail(ApplicationError.Forbidden);

        return Result.Ok(MapToResponse(task));
    }

    /// <summary>
    /// Updates a task's details. Enforces ownership.
    /// </summary>
    public async Task<Result<TaskResponse>> Update(
        Guid taskId,
        UpdateTaskRequest request,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var task = await uow.Tasks.FindAsync(taskId, cancellationToken);

        if (task is null)
            return Result.Fail(ApplicationError.NotFound);

        if (task.UserId != userId)
            return Result.Fail(TaskError.NotOwner);

        var result = task.Update(request.Title, request.Description, request.DueDate);

        if (result.IsFailed)
            return Result.Fail<TaskResponse>(result.Error!.Value);

        await uow.Tasks.UpdateAsync(task, cancellationToken);

        await uow.Commit(cancellationToken);

        return Result.Ok(MapToResponse(task));
    }

    /// <summary>
    /// Changes task status. Enforces ownership and valid transitions.
    /// </summary>
    public async Task<Result> ChangeStatus(
        Guid taskId,
        TaskItemStatus newStatus,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var task = await uow.Tasks.FindAsync(taskId, cancellationToken);

        if (task is null)
            return Result.Fail(ApplicationError.NotFound);

        var result = task.ChangeStatus(newStatus, userId);

        if (result.IsFailed)
            return result;

        await uow.Tasks.UpdateAsync(task, cancellationToken);

        await uow.Commit(cancellationToken);

        return Result.Ok();
    }

    /// <summary>
    /// Deletes a task. Enforces ownership.
    /// </summary>
    public async Task<Result> Delete(
        Guid taskId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var task = await uow.Tasks.FindAsync(taskId, cancellationToken);

        if (task is null)
            return Result.Fail(ApplicationError.NotFound);

        var result = task.Delete(userId);

        if (result.IsFailed)
            return result;

        await uow.Tasks.RemoveAsync(task, cancellationToken);

        await uow.Commit(cancellationToken);

        return Result.Ok();
    }

    private static TaskResponse MapToResponse(TaskItem task) =>
        new(task.Id, task.Title, task.Description, task.Status,
            task.DueDate, task.UserId, task.CreatedAt, task.UpdatedAt);
}
