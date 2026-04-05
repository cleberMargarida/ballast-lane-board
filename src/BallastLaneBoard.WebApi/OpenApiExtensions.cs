using Microsoft.OpenApi;

namespace BallastLaneBoard.WebApi;

internal static class OpenApiExtensions
{
    internal static IServiceCollection AddOpenApiDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

                document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter only the JWT token. Swagger will send it as: Bearer {token}."
                };

                document.Security ??= new List<OpenApiSecurityRequirement>();
                document.Security.Add(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecuritySchemeReference("Bearer", document, null),
                        new List<string>()
                    }
                });

                return Task.CompletedTask;
            });
        });

        return services;
    }

    internal static IApplicationBuilder UseOpenApiDocumentation(this WebApplication app)
    {
        app.MapOpenApi("/api/openapi.json");
        app.UseSwaggerUI(options =>
        {
            options.RoutePrefix = "api";
            options.SwaggerEndpoint("/api/openapi.json", "BallastLaneBoard API v1");
            options.InjectStylesheet("/swagger-dark.css");
            options.OAuthUsePkce();
            options.EnablePersistAuthorization();
        });

        return app;
    }
}
