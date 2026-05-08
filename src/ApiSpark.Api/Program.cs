using ApiSpark.Api.Features.Health;
using ApiSpark.Api.Features.PublicContent;
using ApiSpark.Api.Infrastructure.Auth;
using ApiSpark.Api.Infrastructure.Cors;
using ApiSpark.Api.Infrastructure.Data;
using ApiSpark.Api.Infrastructure.Data.Repositories;
using ApiSpark.Api.Infrastructure.Observability;
using Microsoft.EntityFrameworkCore;

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

// Swagger (dev only)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "ApiSpark API", Version = "v1" });
});

var app = builder.Build();

// Middleware pipeline
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseCors(CorsSetup.PolicyName);
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Route groups
var publicApi = app.MapGroup("/api/public");
publicApi.MapPublicContentApi();

var adminApi = app.MapGroup("/api/admin")
    .RequireAuthorization("AdminOnly");
adminApi.MapAdminHealthApi();

app.MapGroup("/api/publish").RequireAuthorization("Publisher");
app.MapGroup("/api/integrations").RequireAuthorization("ServiceOrAdmin");

// Health
app.MapHealthApi();

// Database initialization
await DatabaseSetup.InitializeAsync(app, app.Lifetime.ApplicationStopping);

await app.RunAsync();

// Required for WebApplicationFactory<Program> in test project (SS-2 fix)
public partial class Program { }
