using System.Net;
using System.Text.Json;
using ApiSpark.Api.Tests.Infrastructure;

namespace ApiSpark.Api.Tests.Features;

[TestClass]
public class LocalSetupTests
{
    private static ApiSparkWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext _)
    {
        _factory = new ApiSparkWebApplicationFactory();
        await _factory.InitializeAsync();
    }

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
        await _factory.DisposeAsync();
    }

    [TestInitialize]
    public void TestInitialize()
    {
        _client = _factory.CreateClient();
    }

    [TestMethod]
    public async Task HealthEndpoint_Responds()
    {
        var response = await _client.GetAsync("/api/health");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task ArticlesEndpoint_ReturnsSeedData()
    {
        var response = await _client.GetAsync("/api/public/content/articles");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var count = doc.RootElement.EnumerateArray().Count();
        Assert.IsTrue(count >= 2, $"Expected at least 2 seeded articles, got {count}");
    }

    [TestMethod]
    public async Task ScalarUiEndpoint_Responds()
    {
        var response = await _client.GetAsync("/scalar/v1");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }
}
