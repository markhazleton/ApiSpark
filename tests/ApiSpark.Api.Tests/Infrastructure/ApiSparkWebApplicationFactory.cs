using System.Security.Claims;
using System.Text.Encodings.Web;
using ApiSpark.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ApiSpark.Api.Tests.Infrastructure;

public class ApiSparkWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private SqliteConnection? _connection;
    private readonly string _dbName = $"TestDb_{Guid.NewGuid():N}";

    public async Task InitializeAsync()
    {
        _connection = new SqliteConnection($"Data Source={_dbName};Mode=Memory;Cache=Shared;");
        await _connection.OpenAsync();
    }

    public new async Task DisposeAsync()
    {
        if (_connection is not null)
            await _connection.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        // Override connection string in configuration so DatabaseSetup reads the test DB path,
        // preventing the path-writability check from running against ./data on developer machines
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = $"Data Source={_dbName};Mode=Memory;Cache=Shared;",
                ["Database:ApplyMigrationsOnStartup"] = "true",
                ["Database:SeedOnStartup"] = "true"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Replace DB context with shared in-memory SQLite
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApiSparkDbContext>));
            if (descriptor is not null) services.Remove(descriptor);

            services.AddDbContext<ApiSparkDbContext>(options =>
                options.UseSqlite($"Data Source={_dbName};Mode=Memory;Cache=Shared;"));

            // Replace JWT Bearer with test auth handler
            services.AddAuthentication("TestScheme")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", null);
        });
    }

    public HttpClient CreateAdminClient()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Claims", "Admin");
        return client;
    }

    public HttpClient CreatePublisherClient()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Claims", "Publisher");
        return client;
    }
}

public class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-Test-Claims", out var roleHeader))
            return Task.FromResult(AuthenticateResult.NoResult());

        var roles = roleHeader.ToString().Split(',', StringSplitOptions.TrimEntries);
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "test-user") };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var identity = new ClaimsIdentity(claims, "TestScheme");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "TestScheme");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
