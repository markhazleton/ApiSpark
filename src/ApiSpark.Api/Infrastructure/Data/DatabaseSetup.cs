using ApiSpark.Api.Infrastructure.Data.Seed;
using Microsoft.EntityFrameworkCore;

namespace ApiSpark.Api.Infrastructure.Data;

public static class DatabaseSetup
{
    public static async Task InitializeAsync(WebApplication app, CancellationToken cancellationToken = default)
    {
        var logger = app.Services.GetRequiredService<ILogger<ApiSparkDbContext>>();

        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApiSparkDbContext>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        if (config.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
        {
            var connStr = config.GetConnectionString("DefaultConnection") ?? "";
            var dataSource = ExtractDataSource(connStr);

            if (!string.IsNullOrEmpty(dataSource) && !dataSource.Contains(":memory:"))
            {
                var dir = Path.GetDirectoryName(dataSource);
                if (!string.IsNullOrEmpty(dir))
                {
                    try
                    {
                        Directory.CreateDirectory(dir);
                        var testFile = Path.Combine(dir, ".write-test");
                        await File.WriteAllTextAsync(testFile, "", cancellationToken);
                        File.Delete(testFile);
                    }
                    catch (Exception ex)
                    {
                        logger.LogCritical(ex, "Database directory '{Dir}' is not writable — aborting startup", dir);
                        throw;
                    }
                }
            }

            try
            {
                await db.Database.MigrateAsync(cancellationToken);
                logger.LogInformation("Database migrations applied successfully");
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Database migration failed — aborting startup");
                throw;
            }
        }

        if (config.GetValue<bool>("Database:SeedOnStartup"))
        {
            if (!await db.Articles.AnyAsync(cancellationToken))
            {
                await SeedData.LoadAsync(db, cancellationToken);
                logger.LogInformation("Seed data loaded");
            }
        }
    }

    private static string ExtractDataSource(string connStr)
    {
        foreach (var part in connStr.Split(';'))
        {
            var kv = part.Split('=', 2);
            if (kv.Length == 2 && kv[0].Trim().Equals("Data Source", StringComparison.OrdinalIgnoreCase))
                return kv[1].Trim();
        }
        return string.Empty;
    }
}
