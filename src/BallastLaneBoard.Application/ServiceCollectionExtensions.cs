using BallastLaneBoard.Application.Identity;
using BallastLaneBoard.Application.TaskManagement;
using Microsoft.Extensions.DependencyInjection;

namespace BallastLaneBoard.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<TaskService>();
        services.AddScoped<UserService>();

        return services;
    }
}
