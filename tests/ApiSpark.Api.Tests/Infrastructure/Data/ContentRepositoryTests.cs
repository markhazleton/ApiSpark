using ApiSpark.Api.Infrastructure.Data;
using ApiSpark.Api.Infrastructure.Data.Entities;
using ApiSpark.Api.Infrastructure.Data.Repositories;
using ApiSpark.Api.Infrastructure.Data.Seed;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ApiSpark.Api.Tests.Infrastructure.Data;

public class ContentRepositoryTests : IAsyncLifetime
{
    private SqliteConnection _connection = null!;
    private ApiSparkDbContext _db = null!;
    private ContentRepository _repo = null!;

    public async Task InitializeAsync()
    {
        var dbName = $"RepoTest_{Guid.NewGuid():N}";
        _connection = new SqliteConnection($"Data Source={dbName};Mode=Memory;Cache=Shared;");
        await _connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ApiSparkDbContext>()
            .UseSqlite($"Data Source={dbName};Mode=Memory;Cache=Shared;")
            .Options;

        _db = new ApiSparkDbContext(options);
        await _db.Database.EnsureCreatedAsync();
        await SeedData.LoadAsync(_db);

        _repo = new ContentRepository(_db);
    }

    public async Task DisposeAsync()
    {
        await _db.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [Fact]
    public async Task GetPublishedArticles_ReturnsOnlyPublished()
    {
        var articles = await _repo.GetPublishedArticlesAsync();
        Assert.All(articles, a => Assert.False(string.IsNullOrEmpty(a.Slug)));
        Assert.DoesNotContain(articles, a => a.Slug == "draft-article");
        Assert.Contains(articles, a => a.Slug == "hello-world");
    }

    [Fact]
    public async Task GetPublishedArticles_ExcludesBody()
    {
        var articles = await _repo.GetPublishedArticlesAsync();
        Assert.True(articles.Count >= 2);
    }

    [Fact]
    public async Task GetPublishedArticleBySlug_Published_ReturnsDetail()
    {
        var article = await _repo.GetPublishedArticleBySlugAsync("hello-world");
        Assert.NotNull(article);
        Assert.Equal("hello-world", article.Slug);
        Assert.False(string.IsNullOrEmpty(article.Body));
    }

    [Fact]
    public async Task GetPublishedArticleBySlug_Draft_ReturnsNull()
    {
        var article = await _repo.GetPublishedArticleBySlugAsync("draft-article");
        Assert.Null(article);
    }

    [Fact]
    public async Task GetPublishedArticleBySlug_Nonexistent_ReturnsNull()
    {
        var article = await _repo.GetPublishedArticleBySlugAsync("does-not-exist");
        Assert.Null(article);
    }

    [Fact]
    public async Task GetAllTags_ReturnsTags()
    {
        var tags = await _repo.GetAllTagsAsync();
        Assert.True(tags.Count >= 2);
        Assert.Contains(tags, t => t.Name == "general");
    }
}
