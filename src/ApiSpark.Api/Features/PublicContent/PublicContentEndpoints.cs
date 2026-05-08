using System.Text.RegularExpressions;

namespace ApiSpark.Api.Features.PublicContent;

public static partial class PublicContentEndpoints
{
    [GeneratedRegex(@"^[a-z0-9][a-z0-9\-]{0,198}[a-z0-9]$|^[a-z0-9]$")]
    private static partial Regex SlugRegex();

    public static RouteGroupBuilder MapPublicContentApi(this RouteGroupBuilder group)
    {
        group.MapGet("/content/articles", async (ContentService svc, CancellationToken ct) =>
        {
            var articles = await svc.GetPublishedArticlesAsync(ct);
            return Results.Ok(articles);
        })
        .WithName("GetPublishedArticles")
        .WithTags("PublicContent")
        .AllowAnonymous();

        group.MapGet("/content/articles/{slug}", async (string slug, ContentService svc, CancellationToken ct) =>
        {
            if (!SlugRegex().IsMatch(slug))
                return Results.Problem("Invalid slug format. Slugs must be lowercase alphanumeric with hyphens, max 200 characters.", statusCode: 400);

            var article = await svc.GetPublishedArticleBySlugAsync(slug, ct);
            return article is null
                ? Results.NotFound(new { message = $"Article '{slug}' not found." })
                : Results.Ok(article);
        })
        .WithName("GetArticleBySlug")
        .WithTags("PublicContent")
        .AllowAnonymous();

        group.MapGet("/content/tags", async (ContentService svc, CancellationToken ct) =>
        {
            var tags = await svc.GetAllTagsAsync(ct);
            return Results.Ok(tags);
        })
        .WithName("GetAllTags")
        .WithTags("PublicContent")
        .AllowAnonymous();

        return group;
    }
}
