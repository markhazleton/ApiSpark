using ApiSpark.Api.Infrastructure.Data;

namespace ApiSpark.Api.Features.Health;

public static class AdminHealthEndpoints
{
    public static RouteGroupBuilder MapAdminHealthApi(this RouteGroupBuilder group)
    {
        group.MapGet("/health/deep", async (ApiSparkDbContext db, CancellationToken ct) =>
        {
            var checks = new Dictionary<string, string>();
            try
            {
                var canConnect = await db.Database.CanConnectAsync(ct);
                checks["database"] = canConnect ? "ok" : "unavailable";
            }
            catch
            {
                checks["database"] = "error";
            }

            var allOk = checks.Values.All(v => v == "ok");
            var response = new DeepHealthResponse(allOk ? "Healthy" : "Degraded", checks);
            return allOk ? Results.Ok(response) : Results.StatusCode(503);
        })
        .WithName("GetDeepHealth")
        .WithTags("Health");

        return group;
    }
}
