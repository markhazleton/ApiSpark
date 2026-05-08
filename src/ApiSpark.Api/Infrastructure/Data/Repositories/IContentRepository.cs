using ApiSpark.Api.Features.PublicContent;

namespace ApiSpark.Api.Infrastructure.Data.Repositories;

public interface IContentRepository
{
    Task<IReadOnlyList<ArticleSummary>> GetPublishedArticlesAsync(CancellationToken ct = default);
    Task<ArticleDetail?> GetPublishedArticleBySlugAsync(string slug, CancellationToken ct = default);
    Task<IReadOnlyList<TagResponse>> GetAllTagsAsync(CancellationToken ct = default);
}
