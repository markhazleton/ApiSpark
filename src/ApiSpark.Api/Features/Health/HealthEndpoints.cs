using System.Reflection;

namespace ApiSpark.Api.Features.Health;

public static class HealthEndpoints
{
    public static WebApplication MapHealthApi(this WebApplication app)
    {
        app.MapGet("/api/health", () =>
        {
            var version = Assembly.GetExecutingAssembly()
                              .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                              ?.InformationalVersion ?? "0.1.0";

            return Results.Ok(new HealthResponse("Healthy", "ApiSpark", version));
        })
        .WithName("GetHealth")
        .WithTags("Health")
        .AllowAnonymous();

        return app;
    }
}
