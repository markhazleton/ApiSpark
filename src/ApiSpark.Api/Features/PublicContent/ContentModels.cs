namespace ApiSpark.Api.Features.PublicContent;

public record ArticleSummary(
    string Slug,
    string Title,
    string Summary,
    DateTimeOffset? PublishDate,
    IReadOnlyList<string> Tags);

public record ArticleDetail(
    string Slug,
    string Title,
    string Summary,
    string Body,
    DateTimeOffset? PublishDate,
    IReadOnlyList<string> Tags);

public record TagResponse(string Name);
