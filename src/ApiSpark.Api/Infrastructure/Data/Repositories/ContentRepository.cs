using ApiSpark.Api.Features.PublicContent;
using ApiSpark.Api.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiSpark.Api.Infrastructure.Data.Repositories;

public class ContentRepository(ApiSparkDbContext db) : IContentRepository
{
    public async Task<IReadOnlyList<ArticleSummary>> GetPublishedArticlesAsync(CancellationToken ct = default)
    {
        // SQLite does not support DateTimeOffset in ORDER BY — sort in memory after fetch
        var articles = await db.Articles
            .Where(a => a.Status == ArticleStatus.Published)
            .Include(a => a.Tags)
            .ToListAsync(ct);

        return articles
            .OrderByDescending(a => a.PublishDate)
            .Select(a => new ArticleSummary(
                a.Slug,
                a.Title,
                a.Summary,
                a.PublishDate,
                a.Tags.Select(t => t.Name).ToList()))
            .ToList();
    }

    public async Task<ArticleDetail?> GetPublishedArticleBySlugAsync(string slug, CancellationToken ct = default)
    {
        var article = await db.Articles
            .Where(a => a.Slug == slug && a.Status == ArticleStatus.Published)
            .Include(a => a.Tags)
            .FirstOrDefaultAsync(ct);

        if (article is null) return null;

        return new ArticleDetail(
            article.Slug,
            article.Title,
            article.Summary,
            article.Body,
            article.PublishDate,
            article.Tags.Select(t => t.Name).ToList());
    }

    public async Task<IReadOnlyList<TagResponse>> GetAllTagsAsync(CancellationToken ct = default)
    {
        return await db.Tags
            .OrderBy(t => t.Name)
            .Select(t => new TagResponse(t.Name))
            .ToListAsync(ct);
    }
}
