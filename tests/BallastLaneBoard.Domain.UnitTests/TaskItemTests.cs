using BallastLaneBoard.Domain.TaskManagement;

namespace BallastLaneBoard.Domain.UnitTests;

public class TaskItemTests
{
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        var result = TaskItem.Create("My task", "desc", null, _userId);

        Assert.True(result.IsSuccess);
        Assert.Equal("My task", result.Value.Title);
        Assert.Equal(TaskItemStatus.Pending, result.Value.Status);
        Assert.Equal(_userId, result.Value.UserId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyTitle_ReturnsFailure(string? title)
    {
        var result = TaskItem.Create(title!, null, null, _userId);

        Assert.True(result.IsFailed);
        Assert.Equal(TaskError.TitleRequired.GetHashCode(), result.Error);
    }

    [Fact]
    public void Create_WithPastDueDate_ReturnsFailure()
    {
        var pastDate = DateTimeOffset.UtcNow.AddDays(-1);

        var result = TaskItem.Create("Task", null, pastDate, _userId);

        Assert.True(result.IsFailed);
        Assert.Equal(TaskError.InvalidDueDate.GetHashCode(), result.Error);
    }

    [Fact]
    public void Create_WithFutureDueDate_ReturnsSuccess()
    {
        var futureDate = DateTimeOffset.UtcNow.AddDays(7);

        var result = TaskItem.Create("Task", null, futureDate, _userId);

        Assert.True(result.IsSuccess);
        Assert.Equal(futureDate, result.Value.DueDate);
    }

    [Fact]
    public void Update_WithValidData_ReturnsSuccess()
    {
        var task = CreateTask();

        var result = task.Update("Updated", "new desc", null);

        Assert.True(result.IsSuccess);
        Assert.Equal("Updated", task.Title);
    }

    [Fact]
    public void Update_WithEmptyTitle_ReturnsFailure()
    {
        var task = CreateTask();

        var result = task.Update("", null, null);

        Assert.True(result.IsFailed);
        Assert.Equal(TaskError.TitleRequired.GetHashCode(), result.Error);
    }

    [Fact]
    public void ChangeStatus_PendingToInProgress_ReturnsSuccess()
    {
        var task = CreateTask();

        var result = task.ChangeStatus(TaskItemStatus.InProgress, _userId);

        Assert.True(result.IsSuccess);
        Assert.Equal(TaskItemStatus.InProgress, task.Status);
    }

    [Fact]
    public void ChangeStatus_InProgressToCompleted_ReturnsSuccess()
    {
        var task = CreateTask();
        task.ChangeStatus(TaskItemStatus.InProgress, _userId);

        var result = task.ChangeStatus(TaskItemStatus.Completed, _userId);

        Assert.True(result.IsSuccess);
        Assert.Equal(TaskItemStatus.Completed, task.Status);
    }

    [Fact]
    public void ChangeStatus_InProgressToPending_ReturnsSuccess()
    {
        var task = CreateTask();
        task.ChangeStatus(TaskItemStatus.InProgress, _userId);

        var result = task.ChangeStatus(TaskItemStatus.Pending, _userId);

        Assert.True(result.IsSuccess);
        Assert.Equal(TaskItemStatus.Pending, task.Status);
    }

    [Fact]
    public void ChangeStatus_PendingToCompleted_ReturnsInvalidTransition()
    {
        var task = CreateTask();

        var result = task.ChangeStatus(TaskItemStatus.Completed, _userId);

        Assert.True(result.IsFailed);
        Assert.Equal(TaskError.InvalidStatusTransition.GetHashCode(), result.Error);
    }

    [Fact]
    public void ChangeStatus_ByNonOwner_ReturnsNotOwner()
    {
        var task = CreateTask();

        var result = task.ChangeStatus(TaskItemStatus.InProgress, Guid.NewGuid());

        Assert.True(result.IsFailed);
        Assert.Equal(TaskError.NotOwner.GetHashCode(), result.Error);
    }

    [Fact]
    public void Delete_ByOwner_ReturnsSuccess()
    {
        var task = CreateTask();

        var result = task.Delete(_userId);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Delete_ByNonOwner_ReturnsNotOwner()
    {
        var task = CreateTask();

        var result = task.Delete(Guid.NewGuid());

        Assert.True(result.IsFailed);
        Assert.Equal(TaskError.NotOwner.GetHashCode(), result.Error);
    }

    private TaskItem CreateTask(string title = "Test Task")
    {
        var result = TaskItem.Create(title, null, null, _userId);
        return result.Value!;
    }
}
