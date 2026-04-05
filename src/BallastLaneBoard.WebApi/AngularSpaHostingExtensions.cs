using System.Text.Json;

namespace BallastLaneBoard.WebApi;

public static class AngularSpaHostingExtensions
{
    private const string DevProxyConfigKey = "Spa:UseProxyInDevelopment";
    private const string SpaProxyContinuePath = "/_spa/continue";
    private const string SpaReturnCookieName = "__blb_spa_return";
    private const string DefaultSpaProxyLaunchPath = "/index.html";

    public static WebApplication UseAngularSpaHosting(this WebApplication app)
    {
        app.UseStaticFiles();
        app.MapAngularSpa();

        return app;
    }

    public static WebApplication MapAngularSpa(this WebApplication app)
    {
        if (ShouldUseSpaDevelopmentProxy(app))
        {
            var proxyLaunchPath = GetSpaProxyLaunchPath();

            app.MapMethods(
                SpaProxyContinuePath,
                [HttpMethods.Get, HttpMethods.Head],
                context =>
                {
                    var returnPath = GetSafeSpaReturnPath(context.Request.Cookies[SpaReturnCookieName]);
                    context.Response.Cookies.Delete(SpaReturnCookieName, new CookieOptions { Path = "/" });
                    context.Response.Redirect(returnPath);
                    return Task.CompletedTask;
                });

            app.MapWhen(
                IsSpaRequest,
                spaApp =>
                {
                    spaApp.Use(async (context, next) =>
                    {
                        try { await next(); }
                        catch (HttpRequestException) when (!context.Response.HasStarted)
                        { await RedirectToSpaProxyLaunchPageAsync(context, proxyLaunchPath); }
                    });

                    spaApp.UseSpa(spa =>
                    {
                        spa.Options.SourcePath = "../BallastLaneBoard.ClientApp";
                        spa.UseProxyToSpaDevelopmentServer("http://localhost:4200");
                    });
                });

            return app;
        }

        MapBuiltSpaFallback(app);
        return app;
    }

    private static bool ShouldUseSpaDevelopmentProxy(WebApplication app)
        => app.Environment.IsDevelopment() && app.Configuration.GetValue<bool>(DevProxyConfigKey);

    private static void MapBuiltSpaFallback(WebApplication app)
    {
        var webRootPath = app.Environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRootPath))
            return;

        var indexPath = Path.Combine(webRootPath, "index.html");

        app.MapFallback(async context =>
        {
            if (!IsSpaRequest(context))
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            if (!File.Exists(indexPath))
            {
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                context.Response.ContentType = "text/plain; charset=utf-8";
                await context.Response.WriteAsync(
                    "SPA assets were not found in wwwroot. Build the Angular app or enable Spa:UseProxyInDevelopment to use the dev server.");
                return;
            }

            context.Response.ContentType = "text/html; charset=utf-8";
            await context.Response.SendFileAsync(indexPath);
        });
    }

    private static bool IsSpaRequest(HttpContext context)
    {
        return (HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsHead(context.Request.Method))
            && !context.Request.Path.StartsWithSegments("/api")
            && !context.Request.Path.StartsWithSegments("/openapi")
            && !context.Request.Path.StartsWithSegments("/health")
            && !context.Request.Path.StartsWithSegments("/swagger")
            && !context.Request.Path.StartsWithSegments(SpaProxyContinuePath);
    }

    private static Task RedirectToSpaProxyLaunchPageAsync(HttpContext context, string spaProxyLaunchPath)
    {
        if (string.Equals(context.Request.Path.Value, spaProxyLaunchPath, StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            return Task.CompletedTask;
        }

        context.Response.Cookies.Append(
            SpaReturnCookieName,
            GetSpaReturnPath(context.Request),
            new CookieOptions
            {
                HttpOnly = true,
                IsEssential = true,
                MaxAge = TimeSpan.FromMinutes(2),
                Path = "/",
                SameSite = SameSiteMode.Lax,
                Secure = context.Request.IsHttps
            });

        context.Response.Redirect(spaProxyLaunchPath);
        return Task.CompletedTask;
    }

    private static string GetSpaReturnPath(HttpRequest request)
        => string.Concat(request.PathBase, request.Path, request.QueryString);

    private static string GetSafeSpaReturnPath(string? returnPath)
    {
        if (string.IsNullOrWhiteSpace(returnPath))
            return "/";

        return returnPath[0] == '/'
            && (returnPath.Length == 1 || (returnPath[1] != '/' && returnPath[1] != '\\'))
                ? returnPath
                : "/";
    }

    private static string GetSpaProxyLaunchPath()
    {
        try
        {
            var configPath = Path.Combine(AppContext.BaseDirectory, "spa.proxy.json");
            if (!File.Exists(configPath))
                return DefaultSpaProxyLaunchPath;

            using var stream = File.OpenRead(configPath);
            using var doc = JsonDocument.Parse(stream);

            if (doc.RootElement.TryGetProperty("SpaProxyServer", out var server)
                && server.TryGetProperty("ServerUrl", out var url)
                && Uri.TryCreate(url.GetString(), UriKind.Absolute, out var uri)
                && !string.IsNullOrWhiteSpace(uri.AbsolutePath))
            {
                return uri.AbsolutePath;
            }

            return DefaultSpaProxyLaunchPath;
        }
        catch
        {
            return DefaultSpaProxyLaunchPath;
        }
    }
}
