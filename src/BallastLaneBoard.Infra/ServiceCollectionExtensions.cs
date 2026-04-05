using BallastLaneBoard.Application.Identity;
using BallastLaneBoard.Application.TaskManagement;
using BallastLaneBoard.Infra.Data;
using BallastLaneBoard.Infra.Keycloak;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace BallastLaneBoard.Infra;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfraServices(this IServiceCollection services, string connectionString)
    {
        var dataSource = NpgsqlDataSource.Create(connectionString);

        services.AddSingleton(dataSource);
        services.AddScoped<ITaskUoW, TaskUoW>();
        services.AddScoped<IUserUoW, UserUoW>();    

        services.AddHostedService<DatabaseMigrationHostedService>();

        services.AddHttpClient<IIdentityProviderClient, KeycloakAdminClient>();

        return services;
    }
}
