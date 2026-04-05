using BallastLaneBoard.Domain.Core;

namespace BallastLaneBoard.Domain.TaskManagement;

/// <summary>
/// Aggregate root representing a task in the system.
/// Business rules: required title, valid status transitions, ownership enforcement, due-date validation.
/// </summary>
public sealed class TaskItem : IAggregateRoot
{
    private TaskItem() { }

    public Guid Id { get; init; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public TaskItemStatus Status { get; private set; }
    public DateTimeOffset? DueDate { get; private set; }
    public Guid UserId { get; private init; }
    public DateTimeOffset CreatedAt { get; private init; }
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>
    /// Factory method — creates a new task assigned to the given user.
    /// </summary>
    public static Result<TaskItem> Create(
        string title,
        string? description,
        DateTimeOffset? dueDate,
        Guid userId)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Fail(TaskError.TitleRequired);

        if (dueDate.HasValue && dueDate.Value < DateTimeOffset.UtcNow)
            return Result.Fail(TaskError.InvalidDueDate);

        var now = DateTimeOffset.UtcNow;

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            Description = description?.Trim(),
            Status = TaskItemStatus.Pending,
            DueDate = dueDate,
            UserId = userId,
            CreatedAt = now,
            UpdatedAt = now
        };

        return Result.Ok(task);
    }

    /// <summary>
    /// Updates task details. Validates domain invariants; caller is responsible for ownership enforcement.
    /// </summary>
    public Result Update(string title, string? description, DateTimeOffset? dueDate)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Fail(TaskError.TitleRequired);

        if (dueDate.HasValue && dueDate.Value < DateTimeOffset.UtcNow)
            return Result.Fail(TaskError.InvalidDueDate);

        Title = title.Trim();
        Description = description?.Trim();
        DueDate = dueDate;
        UpdatedAt = DateTimeOffset.UtcNow;

        return Result.Ok();
    }

    /// <summary>
    /// Transitions task status. Valid transitions:
    /// Pending → InProgress, InProgress → Completed, InProgress → Pending.
    /// </summary>
    public Result ChangeStatus(TaskItemStatus newStatus, Guid requestingUserId)
    {
        if (requestingUserId != UserId)
            return Result.Fail(TaskError.NotOwner);

        if (!IsValidTransition(Status, newStatus))
            return Result.Fail(TaskError.InvalidStatusTransition);

        Status = newStatus;
        UpdatedAt = DateTimeOffset.UtcNow;

        return Result.Ok();
    }

    /// <summary>
    /// Validates ownership for delete operations.
    /// </summary>
    public Result Delete(Guid requestingUserId)
    {
        if (requestingUserId != UserId)
            return Result.Fail(TaskError.NotOwner);

        return Result.Ok();
    }

    private static bool IsValidTransition(TaskItemStatus current, TaskItemStatus next) =>
        (current, next) switch
        {
            (TaskItemStatus.Pending, TaskItemStatus.InProgress) => true,
            (TaskItemStatus.InProgress, TaskItemStatus.Completed) => true,
            (TaskItemStatus.InProgress, TaskItemStatus.Pending) => true,
            _ => false
        };

    /// <summary>
    /// Reconstitutes a TaskItem from persisted data. Used only by the data-access layer.
    /// </summary>
    internal static TaskItem Rehydrate(
        Guid id, string title, string? description, TaskItemStatus status,
        DateTimeOffset? dueDate, Guid userId, DateTimeOffset createdAt, DateTimeOffset updatedAt)
    {
        return new TaskItem
        {
            Id = id,
            Title = title,
            Description = description,
            Status = status,
            DueDate = dueDate,
            UserId = userId,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
    }
}
