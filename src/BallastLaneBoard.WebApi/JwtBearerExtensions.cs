using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using System.Text.Json;

namespace BallastLaneBoard.WebApi;

internal static class JwtBearerExtensions
{
    internal static IServiceCollection AddJwtBearerAuthentication(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var oidcSection = configuration.GetSection("OpenIdConnect");
        var roleClaimPath = oidcSection["RoleClaimPath"] ?? "realm_access.roles";

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = oidcSection["Authority"];
                options.Audience = oidcSection["Audience"];
                var requireHttps = oidcSection["RequireHttpsMetadata"];
                options.RequireHttpsMetadata = requireHttps is not null
                    ? bool.Parse(requireHttps)
                    : !environment.IsDevelopment();
                options.TokenValidationParameters.ValidateIssuer = true;
                options.TokenValidationParameters.ValidateAudience = false;
                // Tokens are issued with the public-facing URL as issuer, but the Authority
                // points to the internal Docker hostname for JWKS/discovery fetching.
                // Override ValidIssuer so both match.
                var publicAuthority = oidcSection["PublicAuthority"];
                if (!string.IsNullOrEmpty(publicAuthority))
                    options.TokenValidationParameters.ValidIssuer = publicAuthority;
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        context.MapTokenRolesToClaims(roleClaimPath);
                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }

    internal static void MapTokenRolesToClaims(this TokenValidatedContext context, string roleClaimPath)
    {
        if (context.Principal?.Identity is not ClaimsIdentity identity)
            return;

        var segments = roleClaimPath.Split('.');
        var claim = context.Principal.FindFirst(segments[0]);
        if (claim is null)
            return;

        if (segments.Length == 1)
        {
            identity.AddRolesFromJson(claim.Value);
            return;
        }

        using var doc = JsonDocument.Parse(claim.Value);
        var current = doc.RootElement;
        for (var i = 1; i < segments.Length; i++)
        {
            if (!current.TryGetProperty(segments[i], out current))
                return;
        }

        if (current.ValueKind == JsonValueKind.Array)
        {
            foreach (var role in current.EnumerateArray())
            {
                var roleName = role.GetString();
                if (roleName is not null)
                    identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
            }
        }
    }

    private static void AddRolesFromJson(this ClaimsIdentity identity, string value)
    {
        try
        {
            using var doc = JsonDocument.Parse(value);
            if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var role in doc.RootElement.EnumerateArray())
                {
                    var roleName = role.GetString();
                    if (roleName is not null)
                        identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
                }
            }
        }
        catch (JsonException)
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, value));
        }
    }
}
