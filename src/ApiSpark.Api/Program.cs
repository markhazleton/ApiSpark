using ApiSpark.Api.Features.Health;
using ApiSpark.Api.Features.PublicContent;
using ApiSpark.Api.Features.Recipe;
using ApiSpark.Api.Infrastructure.Auth;
using ApiSpark.Api.Infrastructure.Cors;
using ApiSpark.Api.Infrastructure.Data;
using ApiSpark.Api.Infrastructure.Data.Repositories;
using ApiSpark.Api.Infrastructure.Observability;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using WebSpark.Recipe.Data;
using WebSpark.Recipe.Interfaces;
using WebSpark.Recipe.Providers;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseShutdownTimeout(TimeSpan.FromSeconds(15));

// Auth (must register scheme + policies — SS-1 fix)
builder.Services.AddApiSparkAuth();

// CORS
builder.Services.AddApiSparkCors(builder.Configuration, builder.Environment);

// Data layer
builder.Services.AddDbContext<ApiSparkDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IContentRepository, ContentRepository>();
builder.Services.AddScoped<ContentService>();

// Recipe data layer
builder.Services.AddDbContext<RecipeDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("RecipeConnection")));
builder.Services.AddScoped<IRecipeService, RecipeProvider>();
builder.Services.AddScoped<RecipeService>();

// OpenAPI / Scalar (dev only)
builder.Services.AddOpenApi();

var app = builder.Build();

// Middleware pipeline
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseCors(CorsSetup.PolicyName);
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

var adminApi = app.MapGroup("/api/admin")
    .RequireAuthorization("AdminOnly");
adminApi.MapAdminHealthApi();

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
