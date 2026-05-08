using System.Diagnostics;
using System.Net;
using System.Text.Json;
using ApiSpark.Api.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ApiSpark.Api.Tests.Features.Health;

public class HealthEndpointTests(ApiSparkWebApplicationFactory factory)
    : IClassFixture<ApiSparkWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetHealth_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetHealth_ReturnsCorrectBody()
    {
        var response = await _client.GetAsync("/api/health");
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        Assert.Equal("Healthy", doc.RootElement.GetProperty("status").GetString());
        Assert.Equal("ApiSpark", doc.RootElement.GetProperty("service").GetString());
        Assert.False(string.IsNullOrEmpty(doc.RootElement.GetProperty("version").GetString()));
    }

    [Fact]
    public async Task GetHealth_IsAnonymous_NoAuthRequired()
    {
        var anonClient = factory.CreateClient();
        var response = await anonClient.GetAsync("/api/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetHealth_RespondsFastEnough()
    {
        var sw = Stopwatch.StartNew();
        await _client.GetAsync("/api/health");
        sw.Stop();
        Assert.True(sw.ElapsedMilliseconds < 500, $"Health response exceeded 500ms: {sw.ElapsedMilliseconds}ms");
    }
}
