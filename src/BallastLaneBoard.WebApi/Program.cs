using BallastLaneBoard.Application;
using BallastLaneBoard.Infra;
using BallastLaneBoard.WebApi;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ── MVC + OpenAPI ──

builder.Services.AddControllers().AddJsonOptions(
    options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddOpenApiDocumentation();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();

// ── Authentication (OIDC JWT) ──

builder.Services.AddJwtBearerAuthentication(builder.Configuration, builder.Environment);

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("admin"));
    options.AddPolicy("User", policy => policy.RequireRole("user"));
});

// ── Application + Infrastructure ──

builder.Services.AddApplicationServices();
builder.Services.AddInfraServices(builder.Configuration.GetConnectionString("DefaultConnection")!);

// ── Swagger UI (dark theme + bearer token) ──

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
}

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.UseOpenApiDocumentation();

app.UseAngularSpaHosting();
app.MapHealthChecks("/health");

app.MapGet("/api/health", () => Results.Ok(new
{
    status = "Healthy",
    timestamp = DateTimeOffset.UtcNow
})).AllowAnonymous();

app.MapControllers();

app.MapGet("/api/config", (IConfiguration config) => Results.Ok(new
{
    issuer = config["OpenIdConnect:PublicAuthority"] ?? config["OpenIdConnect:Authority"]
})).AllowAnonymous();

app.Run();


