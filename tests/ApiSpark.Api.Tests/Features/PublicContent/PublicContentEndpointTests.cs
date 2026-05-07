using System.Net;
using System.Text.Json;
using ApiSpark.Api.Infrastructure.Data;
using ApiSpark.Api.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace ApiSpark.Api.Tests.Features.PublicContent;

public class PublicContentEndpointTests : IClassFixture<ApiSparkWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly ApiSparkWebApplicationFactory _factory;

    public PublicContentEndpointTests(ApiSparkWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetArticles_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/public/content/articles");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetArticles_ReturnsOnlyPublished()
    {
        var response = await _client.GetAsync("/api/public/content/articles");
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var articles = doc.RootElement.EnumerateArray().ToList();

        Assert.All(articles, a =>
        {
            Assert.True(a.TryGetProperty("slug", out _));
            Assert.True(a.TryGetProperty("title", out _));
            Assert.True(a.TryGetProperty("summary", out _));
            Assert.True(a.TryGetProperty("tags", out _));
            Assert.False(a.TryGetProperty("body", out _), "Body should not appear in list response");
        });
        Assert.DoesNotContain(articles, a => a.GetProperty("slug").GetString() == "draft-article");
    }

    [Fact]
    public async Task GetArticleBySlug_Published_ReturnsFullDetail()
    {
        var response = await _client.GetAsync("/api/public/content/articles/hello-world");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.TryGetProperty("body", out var body));
        Assert.False(string.IsNullOrEmpty(body.GetString()));
    }

    [Fact]
    public async Task GetArticleBySlug_Draft_Returns404()
    {
        var response = await _client.GetAsync("/api/public/content/articles/draft-article");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetArticleBySlug_Nonexistent_Returns404()
    {
        var response = await _client.GetAsync("/api/public/content/articles/does-not-exist");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetArticleBySlug_InvalidSlug_Returns400()
    {
        var response = await _client.GetAsync("/api/public/content/articles/INVALID_SLUG!");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetTags_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/public/content/tags");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
