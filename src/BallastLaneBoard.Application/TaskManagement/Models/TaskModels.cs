using BallastLaneBoard.Domain.TaskManagement;

namespace BallastLaneBoard.Application.TaskManagement.Models;

public sealed record CreateTaskRequest(string Title, string? Description, DateTimeOffset? DueDate);

public sealed record UpdateTaskRequest(string Title, string? Description, DateTimeOffset? DueDate);

public sealed record ChangeTaskStatusRequest(TaskItemStatus Status);

public sealed record TaskResponse(
    Guid Id,
    string Title,
    string? Description,
    TaskItemStatus Status,
    DateTimeOffset? DueDate,
    Guid UserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
