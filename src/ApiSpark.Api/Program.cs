using ApiSpark.Api.Features.Health;
using ApiSpark.Api.Features.PublicContent;
using ApiSpark.Api.Features.Recipe;
using ApiSpark.Api.Features.WebSpark;
using ApiSpark.Api.Infrastructure.Auth;
using ApiSpark.Api.Infrastructure.Cors;
using ApiSpark.Api.Infrastructure.Data;
using ApiSpark.Api.Infrastructure.Data.Repositories;
using ApiSpark.Api.Infrastructure.Observability;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using WebSpark.Core.Data;
using WebSpark.Recipe.Data;
using WebSpark.Recipe.Interfaces;
using WebSpark.Recipe.Providers;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseShutdownTimeout(TimeSpan.FromSeconds(15));

// Auth (must register scheme + policies — SS-1 fix)
builder.Services.AddApiSparkAuth();

// CORS
builder.Services.AddApiSparkCors(builder.Configuration, builder.Environment);

// Resolve a SQLite "Data Source=<path>" against ContentRootPath so relative
// paths work correctly under IIS (which sets a different working directory).
var contentRoot = builder.Environment.ContentRootPath;
static string ResolveSqliteConnStr(string? connStr, string root)
{
    if (string.IsNullOrEmpty(connStr)) return string.Empty;
    var parts = connStr.Split(';', StringSplitOptions.RemoveEmptyEntries);
    return string.Join(';', parts.Select(part =>
    {
        var kv = part.Split('=', 2);
        if (kv.Length == 2 && kv[0].Trim().Equals("Data Source", StringComparison.OrdinalIgnoreCase))
        {
            var src = kv[1].Trim();
            if (!Path.IsPathRooted(src))
                src = Path.GetFullPath(Path.Combine(root, src));
            return $"Data Source={src}";
        }
        return part;
    }));
}

// Data layer
builder.Services.AddDbContext<ApiSparkDbContext>(options =>
    options.UseSqlite(ResolveSqliteConnStr(builder.Configuration.GetConnectionString("DefaultConnection"), contentRoot)));

builder.Services.AddScoped<IContentRepository, ContentRepository>();
builder.Services.AddScoped<ContentService>();

// Recipe data layer
builder.Services.AddDbContext<RecipeDbContext>(options =>
    options.UseSqlite(ResolveSqliteConnStr(builder.Configuration.GetConnectionString("RecipeConnection"), contentRoot)));
builder.Services.AddScoped<IRecipeService, RecipeProvider>();
builder.Services.AddScoped<RecipeService>();

// WebSpark.Core data layer
builder.Services.AddDbContext<WebSparkDbContext>(options =>
    options.UseSqlite(ResolveSqliteConnStr(builder.Configuration.GetConnectionString("WebSparkConnection"), contentRoot))
           .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));
builder.Services.AddScoped<WebSparkService>();

// OpenAPI / Scalar (dev only)
builder.Services.AddOpenApi();

var app = builder.Build();

// Middleware pipeline
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseCors(CorsSetup.PolicyName);
app.UseDefaultFiles();   // serves wwwroot/index.html at "/"
app.UseStaticFiles();    // serves wwwroot/**
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();                // /openapi/v1.json
    app.MapScalarApiReference();     // /scalar/v1
}

// Route groups
var publicApi = app.MapGroup("/api/public");
publicApi.MapPublicContentApi();
publicApi.MapPublicRecipeApi();
publicApi.MapGroup("/webspark").MapPublicWebSparkApi();

var adminApi = app.MapGroup("/api/admin")
    .RequireAuthorization("AdminOnly");
adminApi.MapAdminHealthApi();
adminApi.MapGroup("/webspark").MapAdminWebSparkApi();

var publishApi = app.MapGroup("/api/publish").RequireAuthorization("Publisher");
publishApi.MapPublishRecipeApi();

app.MapGroup("/api/integrations").RequireAuthorization("ServiceOrAdmin");

// Health
app.MapHealthApi();

// Database initialization
await DatabaseSetup.InitializeAsync(app, app.Lifetime.ApplicationStopping);

await app.RunAsync();

// Required for WebApplicationFactory<Program> in test project (SS-2 fix)
public partial class Program { }
