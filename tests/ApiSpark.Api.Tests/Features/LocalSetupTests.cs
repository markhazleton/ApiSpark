using System.Net;
using System.Text.Json;
using ApiSpark.Api.Tests.Infrastructure;

namespace ApiSpark.Api.Tests.Features;

public class LocalSetupTests(ApiSparkWebApplicationFactory factory)
    : IClassFixture<ApiSparkWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task HealthEndpoint_Responds()
    {
        var response = await _client.GetAsync("/api/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ArticlesEndpoint_ReturnsSeedData()
    {
        var response = await _client.GetAsync("/api/public/content/articles");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var count = doc.RootElement.EnumerateArray().Count();
        Assert.True(count >= 2, $"Expected at least 2 seeded articles, got {count}");
    }

    [Fact]
    public async Task ScalarUiEndpoint_Responds()
    {
        var response = await _client.GetAsync("/scalar/v1");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
