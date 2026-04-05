using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BallastLaneBoard.Application.TaskManagement.Models;
using BallastLaneBoard.WebApi.IntegrationTests.Infrastructure;

namespace BallastLaneBoard.WebApi.IntegrationTests;

[Collection("Integration")]
public class TasksControllerTests(ApiFactory api)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    [Fact]
    public async Task GetAll_WithoutAuth_Returns401()
    {
        var ct = TestContext.Current.CancellationToken;
        var response = await api.CreateClient().GetAsync("/api/tasks", ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_Authenticated_Returns200()
    {
        var ct = TestContext.Current.CancellationToken;
        var client = await CreateUser1ClientAsync(ct);

        var response = await client.GetAsync("/api/tasks", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Create_ValidRequest_Returns201()
    {
        var ct = TestContext.Current.CancellationToken;
        var client = await CreateUser1ClientAsync(ct);
        var request = new CreateTaskRequest("Integration Task", "desc", null);

        var response = await client.PostAsJsonAsync("/api/tasks", request, ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var task = await response.Content.ReadFromJsonAsync<TaskResponse>(JsonOptions, ct);
        Assert.NotNull(task);
        Assert.Equal("Integration Task", task.Title);
    }

    [Fact]
    public async Task Create_EmptyTitle_Returns400()
    {
        var ct = TestContext.Current.CancellationToken;
        var client = await CreateUser1ClientAsync(ct);
        var request = new CreateTaskRequest("", null, null);

        var response = await client.PostAsJsonAsync("/api/tasks", request, ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetById_OwnTask_Returns200()
    {
        var ct = TestContext.Current.CancellationToken;
        var client = await CreateUser1ClientAsync(ct);
        var createResponse = await client.PostAsJsonAsync("/api/tasks",
            new CreateTaskRequest("Get Test", null, null), ct);
        var created = await createResponse.Content.ReadFromJsonAsync<TaskResponse>(JsonOptions, ct);

        var response = await client.GetAsync($"/api/tasks/{created!.Id}", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_OtherUsersTask_Returns403()
    {
        var ct = TestContext.Current.CancellationToken;
        // User1 creates a task; User2 (different Keycloak identity) tries to access it.
        var ownerClient = await CreateUser1ClientAsync(ct);
        var createResponse = await ownerClient.PostAsJsonAsync("/api/tasks",
            new CreateTaskRequest("Owner Task", null, null), ct);
        var created = await createResponse.Content.ReadFromJsonAsync<TaskResponse>(JsonOptions, ct);

        var otherClient = await CreateUser2ClientAsync(ct);
        var response = await otherClient.GetAsync($"/api/tasks/{created!.Id}", ct);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetById_AsAdmin_CanSeeOtherUsersTask()
    {
        var ct = TestContext.Current.CancellationToken;
        var ownerClient = await CreateUser1ClientAsync(ct);
        var createResponse = await ownerClient.PostAsJsonAsync("/api/tasks",
            new CreateTaskRequest("Admin View Task", null, null), ct);
        var created = await createResponse.Content.ReadFromJsonAsync<TaskResponse>(JsonOptions, ct);

        // Admin has the "admin" realm role in Keycloak → isAdmin=true → bypasses ownership check.
        var adminClient = await CreateAdminClientAsync(ct);
        var response = await adminClient.GetAsync($"/api/tasks/{created!.Id}", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Delete_OwnTask_Returns204()
    {
        var ct = TestContext.Current.CancellationToken;
        var client = await CreateUser1ClientAsync(ct);
        var createResponse = await client.PostAsJsonAsync("/api/tasks",
            new CreateTaskRequest("Delete Task", null, null), ct);
        var created = await createResponse.Content.ReadFromJsonAsync<TaskResponse>(JsonOptions, ct);

        var response = await client.DeleteAsync($"/api/tasks/{created!.Id}", ct);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_NonExistent_Returns404()
    {
        var ct = TestContext.Current.CancellationToken;
        var client = await CreateUser1ClientAsync(ct);

        var response = await client.DeleteAsync($"/api/tasks/{Guid.NewGuid()}", ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ChangeStatus_ValidTransition_Returns204()
    {
        var ct = TestContext.Current.CancellationToken;
        var client = await CreateUser1ClientAsync(ct);
        var createResponse = await client.PostAsJsonAsync("/api/tasks",
            new CreateTaskRequest("Status Task", null, null), ct);
        var created = await createResponse.Content.ReadFromJsonAsync<TaskResponse>(JsonOptions, ct);

        var response = await client.PatchAsJsonAsync(
            $"/api/tasks/{created!.Id}/status",
            new ChangeTaskStatusRequest(Domain.TaskManagement.TaskItemStatus.InProgress),
            JsonOptions,
            ct);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private Task<HttpClient> CreateUser1ClientAsync(CancellationToken ct) =>
        api.CreateAuthenticatedClientAsync(ApiFactory.User1Name, ApiFactory.User1Password, ct);

    private Task<HttpClient> CreateUser2ClientAsync(CancellationToken ct) =>
        api.CreateAuthenticatedClientAsync(ApiFactory.User2Name, ApiFactory.User2Password, ct);

    private Task<HttpClient> CreateAdminClientAsync(CancellationToken ct) =>
        api.CreateAuthenticatedClientAsync(ApiFactory.AdminName, ApiFactory.AdminPassword, ct);
}
