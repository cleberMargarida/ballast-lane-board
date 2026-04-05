using BallastLaneBoard.Application.Abstractions;
using BallastLaneBoard.Application.TaskManagement;
using BallastLaneBoard.Application.TaskManagement.Models;
using BallastLaneBoard.Application.UnitTests.Infrastructure;
using BallastLaneBoard.Domain.TaskManagement;

namespace BallastLaneBoard.Application.UnitTests;

public class TaskServiceTests
{
    private readonly InMemoryTaskUoW _uow = new();
    private readonly TaskService _sut;
    private readonly Guid _userId = Guid.NewGuid();

    public TaskServiceTests()
    {
        _sut = new TaskService(_uow);
    }

    [Fact]
    public async Task Create_WithValidRequest_ReturnsSuccessAndCommits()
    {
        var request = new CreateTaskRequest("Test Task", "Description", null);

        var result = await _sut.Create(request, _userId, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Test Task", result.Value!.Title);
        Assert.Single(_uow.TasksStore.Items);
        Assert.Equal(1, _uow.CommitCount);
    }

    [Fact]
    public async Task Create_WithEmptyTitle_ReturnsFailure()
    {
        var request = new CreateTaskRequest("", null, null);

        var result = await _sut.Create(request, _userId, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Empty(_uow.TasksStore.Items);
        Assert.Equal(0, _uow.CommitCount);
    }

    [Fact]
    public async Task GetAll_AsRegularUser_ReturnsOwnTasksOnly()
    {
        var otherId = Guid.NewGuid();
        await _sut.Create(new CreateTaskRequest("My Task", null, null), _userId, CancellationToken.None);
        await _sut.Create(new CreateTaskRequest("Other Task", null, null), otherId, CancellationToken.None);

        var tasks = await _sut.GetAll(_userId, isAdmin: false, CancellationToken.None);

        Assert.Single(tasks);
        Assert.Equal("My Task", tasks[0].Title);
    }

    [Fact]
    public async Task GetAll_AsAdmin_ReturnsAllTasks()
    {
        var otherId = Guid.NewGuid();
        await _sut.Create(new CreateTaskRequest("My Task", null, null), _userId, CancellationToken.None);
        await _sut.Create(new CreateTaskRequest("Other Task", null, null), otherId, CancellationToken.None);

        var tasks = await _sut.GetAll(_userId, isAdmin: true, CancellationToken.None);

        Assert.Equal(2, tasks.Count);
    }

    [Fact]
    public async Task GetById_ExistingOwnTask_ReturnsSuccess()
    {
        var created = await _sut.Create(new CreateTaskRequest("Task", null, null), _userId, CancellationToken.None);
        var taskId = created.Value!.Id;

        var result = await _sut.GetById(taskId, _userId, isAdmin: false, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Task", result.Value!.Title);
    }

    [Fact]
    public async Task GetById_OtherUsersTask_ReturnsForbidden()
    {
        var created = await _sut.Create(new CreateTaskRequest("Task", null, null), _userId, CancellationToken.None);
        var taskId = created.Value!.Id;

        var result = await _sut.GetById(taskId, Guid.NewGuid(), isAdmin: false, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Equal(ApplicationError.Forbidden.GetHashCode(), result.Error);
    }

    [Fact]
    public async Task GetById_AsAdmin_CanSeeOtherUsersTask()
    {
        var created = await _sut.Create(new CreateTaskRequest("Task", null, null), _userId, CancellationToken.None);
        var taskId = created.Value!.Id;

        var result = await _sut.GetById(taskId, Guid.NewGuid(), isAdmin: true, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task GetById_NonExistent_ReturnsNotFound()
    {
        var result = await _sut.GetById(Guid.NewGuid(), _userId, false, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Equal(ApplicationError.NotFound.GetHashCode(), result.Error);
    }

    [Fact]
    public async Task Update_OwnTask_ReturnsSuccess()
    {
        var created = await _sut.Create(new CreateTaskRequest("Task", null, null), _userId, CancellationToken.None);
        var taskId = created.Value!.Id;

        var result = await _sut.Update(
            taskId, new UpdateTaskRequest("Updated", "desc", null), _userId, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Updated", result.Value!.Title);
    }

    [Fact]
    public async Task Update_OtherUsersTask_ReturnsNotOwner()
    {
        var created = await _sut.Create(new CreateTaskRequest("Task", null, null), _userId, CancellationToken.None);
        var taskId = created.Value!.Id;

        var result = await _sut.Update(
            taskId, new UpdateTaskRequest("Updated", null, null), Guid.NewGuid(), CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Equal(TaskError.NotOwner.GetHashCode(), result.Error);
    }

    [Fact]
    public async Task ChangeStatus_ValidTransition_ReturnsSuccess()
    {
        var created = await _sut.Create(new CreateTaskRequest("Task", null, null), _userId, CancellationToken.None);
        var taskId = created.Value!.Id;

        var result = await _sut.ChangeStatus(
            taskId, TaskItemStatus.InProgress, _userId, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ChangeStatus_InvalidTransition_ReturnsFailure()
    {
        var created = await _sut.Create(new CreateTaskRequest("Task", null, null), _userId, CancellationToken.None);
        var taskId = created.Value!.Id;

        var result = await _sut.ChangeStatus(
            taskId, TaskItemStatus.Completed, _userId, CancellationToken.None);

        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task Delete_OwnTask_ReturnsSuccessAndRemoves()
    {
        var created = await _sut.Create(new CreateTaskRequest("Task", null, null), _userId, CancellationToken.None);
        var taskId = created.Value!.Id;
        var commitsBefore = _uow.CommitCount;

        var result = await _sut.Delete(taskId, _userId, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(_uow.TasksStore.Items);
        Assert.Equal(commitsBefore + 1, _uow.CommitCount);
    }

    [Fact]
    public async Task Delete_NonExistent_ReturnsNotFound()
    {
        var result = await _sut.Delete(Guid.NewGuid(), _userId, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Equal(ApplicationError.NotFound.GetHashCode(), result.Error);
    }
}
