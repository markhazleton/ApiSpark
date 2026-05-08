using ApiSpark.Api.Infrastructure.Data.Entities;

namespace ApiSpark.Api.Infrastructure.Data.Seed;

public static class SeedData
{
    public static async Task LoadAsync(ApiSparkDbContext db, CancellationToken ct = default)
    {
        var tagGeneral = new Tag { Name = "general" };
        var tagIntro = new Tag { Name = "intro" };
        var tagApispark = new Tag { Name = "apispark" };
        var tagTutorial = new Tag { Name = "tutorial" };

        db.Tags.AddRange(tagGeneral, tagIntro, tagApispark, tagTutorial);

        db.Articles.AddRange(
            new Article
            {
                Slug = "hello-world",
                Title = "Hello World",
                Summary = "Introduction to ApiSpark — the modular .NET API platform.",
                Body = "# Hello World\n\nWelcome to ApiSpark. This is the first published article demonstrating the public content feature.\n\n## What is ApiSpark?\n\nApiSpark is a modular ASP.NET Core backend API platform for small personal and portfolio APIs hosted on Azure App Service.\n",
                Status = ArticleStatus.Published,
                PublishDate = DateTimeOffset.UtcNow.AddDays(-7),
                Tags = [tagGeneral, tagIntro]
            },
            new Article
            {
                Slug = "getting-started-with-apispark",
                Title = "Getting Started with ApiSpark",
                Summary = "A step-by-step guide to running ApiSpark locally and exploring the public API.",
                Body = "# Getting Started with ApiSpark\n\nThis guide walks you through cloning the repository and running the API locally.\n\n## Prerequisites\n\n- .NET 10 SDK\n- Git\n\n## Quick Start\n\n```bash\ngit clone https://github.com/MarkHazleton/ApiSpark.git\ncd ApiSpark\ndotnet run --project src/ApiSpark.Api\n```\n\nVisit `http://localhost:5000/api/health` to verify the API is running.\n",
                Status = ArticleStatus.Published,
                PublishDate = DateTimeOffset.UtcNow.AddDays(-3),
                Tags = [tagApispark, tagTutorial]
            },
            new Article
            {
                Slug = "draft-article",
                Title = "Draft Article",
                Summary = "This article is a draft and should not appear in public endpoints.",
                Body = "# Draft Article\n\nThis content is not yet published.\n",
                Status = ArticleStatus.Draft,
                Tags = [tagGeneral]
            });

        await db.SaveChangesAsync(ct);
    }
}
