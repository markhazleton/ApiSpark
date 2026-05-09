using WebSpark.Core.Data;

namespace ApiSpark.Api.Features.WebSpark;

public static class WebSparkEndpoints
{
    public static RouteGroupBuilder MapPublicWebSparkApi(this RouteGroupBuilder group)
    {
        // Domain / WebSites
        group.MapGet("/domains", async (WebSparkService svc, CancellationToken ct) =>
            Results.Ok(await svc.GetDomainsAsync(ct)))
            .WithName("GetDomains").WithTags("WebSpark-Domains").AllowAnonymous();

        group.MapGet("/domains/{id:int}", async (int id, WebSparkService svc, CancellationToken ct) =>
        {
            var item = await svc.GetDomainAsync(id, ct);
            return item is null ? Results.NotFound() : Results.Ok(item);
        }).WithName("GetDomain").WithTags("WebSpark-Domains").AllowAnonymous();

        // Blogs
        group.MapGet("/blogs", async (WebSparkService svc, CancellationToken ct) =>
            Results.Ok(await svc.GetBlogsAsync(ct)))
            .WithName("GetBlogs").WithTags("WebSpark-Blogs").AllowAnonymous();

        group.MapGet("/blogs/{id:int}", async (int id, WebSparkService svc, CancellationToken ct) =>
        {
            var item = await svc.GetBlogAsync(id, ct);
            return item is null ? Results.NotFound() : Results.Ok(item);
        }).WithName("GetBlog").WithTags("WebSpark-Blogs").AllowAnonymous();

        // Authors
        group.MapGet("/authors", async (WebSparkService svc, CancellationToken ct) =>
            Results.Ok(await svc.GetAuthorsAsync(ct)))
            .WithName("GetAuthors").WithTags("WebSpark-Authors").AllowAnonymous();

        group.MapGet("/authors/{id:int}", async (int id, WebSparkService svc, CancellationToken ct) =>
        {
            var item = await svc.GetAuthorAsync(id, ct);
            return item is null ? Results.NotFound() : Results.Ok(item);
        }).WithName("GetAuthor").WithTags("WebSpark-Authors").AllowAnonymous();

        // Posts
        group.MapGet("/posts", async (int? blogId, WebSparkService svc, CancellationToken ct) =>
            Results.Ok(await svc.GetPostsAsync(blogId, ct)))
            .WithName("GetPosts").WithTags("WebSpark-Posts").AllowAnonymous();

        group.MapGet("/posts/{id:int}", async (int id, WebSparkService svc, CancellationToken ct) =>
        {
            var item = await svc.GetPostAsync(id, ct);
            return item is null ? Results.NotFound() : Results.Ok(item);
        }).WithName("GetPost").WithTags("WebSpark-Posts").AllowAnonymous();

        // Categories
        group.MapGet("/categories", async (WebSparkService svc, CancellationToken ct) =>
            Results.Ok(await svc.GetCategoriesAsync(ct)))
            .WithName("GetCategories").WithTags("WebSpark-Categories").AllowAnonymous();

        group.MapGet("/categories/{id:int}", async (int id, WebSparkService svc, CancellationToken ct) =>
        {
            var item = await svc.GetCategoryAsync(id, ct);
            return item is null ? Results.NotFound() : Results.Ok(item);
        }).WithName("GetCategory").WithTags("WebSpark-Categories").AllowAnonymous();

        // Menu
        group.MapGet("/menus", async (int? domainId, WebSparkService svc, CancellationToken ct) =>
            Results.Ok(await svc.GetMenusAsync(domainId, ct)))
            .WithName("GetMenus").WithTags("WebSpark-Menu").AllowAnonymous();

        group.MapGet("/menus/{id:int}", async (int id, WebSparkService svc, CancellationToken ct) =>
        {
            var item = await svc.GetMenuAsync(id, ct);
            return item is null ? Results.NotFound() : Results.Ok(item);
        }).WithName("GetMenu").WithTags("WebSpark-Menu").AllowAnonymous();

        // Keywords
        group.MapGet("/keywords", async (WebSparkService svc, CancellationToken ct) =>
            Results.Ok(await svc.GetKeywordsAsync(ct)))
            .WithName("GetKeywords").WithTags("WebSpark-Keywords").AllowAnonymous();

        group.MapGet("/keywords/{id:int}", async (int id, WebSparkService svc, CancellationToken ct) =>
        {
            var item = await svc.GetKeywordAsync(id, ct);
            return item is null ? Results.NotFound() : Results.Ok(item);
        }).WithName("GetKeyword").WithTags("WebSpark-Keywords").AllowAnonymous();

        // ContentParts
        group.MapGet("/content-parts", async (WebSparkService svc, CancellationToken ct) =>
            Results.Ok(await svc.GetContentPartsAsync(ct)))
            .WithName("GetContentParts").WithTags("WebSpark-ContentParts").AllowAnonymous();

        group.MapGet("/content-parts/{id:int}", async (int id, WebSparkService svc, CancellationToken ct) =>
        {
            var item = await svc.GetContentPartAsync(id, ct);
            return item is null ? Results.NotFound() : Results.Ok(item);
        }).WithName("GetContentPart").WithTags("WebSpark-ContentParts").AllowAnonymous();

        return group;
    }

    public static RouteGroupBuilder MapAdminWebSparkApi(this RouteGroupBuilder group)
    {
        // Domain CRUD
        group.MapPost("/domains", async (WebSite entity, WebSparkService svc, CancellationToken ct) =>
        {
            var created = await svc.CreateDomainAsync(entity, ct);
            return Results.Created($"/api/public/webspark/domains/{created.Id}", created);
        }).WithName("CreateDomain").WithTags("WebSpark-Domains");

        group.MapPut("/domains/{id:int}", async (int id, WebSite entity, WebSparkService svc, CancellationToken ct) =>
        {
            var updated = await svc.UpdateDomainAsync(id, entity, ct);
            return updated is null ? Results.NotFound() : Results.Ok(updated);
        }).WithName("UpdateDomain").WithTags("WebSpark-Domains");

        group.MapDelete("/domains/{id:int}", async (int id, WebSparkService svc, CancellationToken ct) =>
            await svc.DeleteDomainAsync(id, ct) ? Results.NoContent() : Results.NotFound())
            .WithName("DeleteDomain").WithTags("WebSpark-Domains");

        // Blog CRUD
        group.MapPost("/blogs", async (Blog entity, WebSparkService svc, CancellationToken ct) =>
        {
            var created = await svc.CreateBlogAsync(entity, ct);
            return Results.Created($"/api/public/webspark/blogs/{created.Id}", created);
        }).WithName("CreateBlog").WithTags("WebSpark-Blogs");

        group.MapPut("/blogs/{id:int}", async (int id, Blog entity, WebSparkService svc, CancellationToken ct) =>
        {
            var updated = await svc.UpdateBlogAsync(id, entity, ct);
            return updated is null ? Results.NotFound() : Results.Ok(updated);
        }).WithName("UpdateBlog").WithTags("WebSpark-Blogs");

        group.MapDelete("/blogs/{id:int}", async (int id, WebSparkService svc, CancellationToken ct) =>
            await svc.DeleteBlogAsync(id, ct) ? Results.NoContent() : Results.NotFound())
            .WithName("DeleteBlog").WithTags("WebSpark-Blogs");

        // Author CRUD
        group.MapPost("/authors", async (Author entity, WebSparkService svc, CancellationToken ct) =>
        {
            var created = await svc.CreateAuthorAsync(entity, ct);
            return Results.Created($"/api/public/webspark/authors/{created.Id}", created);
        }).WithName("CreateAuthor").WithTags("WebSpark-Authors");

        group.MapPut("/authors/{id:int}", async (int id, Author entity, WebSparkService svc, CancellationToken ct) =>
        {
            var updated = await svc.UpdateAuthorAsync(id, entity, ct);
            return updated is null ? Results.NotFound() : Results.Ok(updated);
        }).WithName("UpdateAuthor").WithTags("WebSpark-Authors");

        group.MapDelete("/authors/{id:int}", async (int id, WebSparkService svc, CancellationToken ct) =>
            await svc.DeleteAuthorAsync(id, ct) ? Results.NoContent() : Results.NotFound())
            .WithName("DeleteAuthor").WithTags("WebSpark-Authors");

        // Post CRUD
        group.MapPost("/posts", async (Post entity, WebSparkService svc, CancellationToken ct) =>
        {
            var created = await svc.CreatePostAsync(entity, ct);
            return Results.Created($"/api/public/webspark/posts/{created.Id}", created);
        }).WithName("CreatePost").WithTags("WebSpark-Posts");

        group.MapPut("/posts/{id:int}", async (int id, Post entity, WebSparkService svc, CancellationToken ct) =>
        {
            var updated = await svc.UpdatePostAsync(id, entity, ct);
            return updated is null ? Results.NotFound() : Results.Ok(updated);
        }).WithName("UpdatePost").WithTags("WebSpark-Posts");

        group.MapDelete("/posts/{id:int}", async (int id, WebSparkService svc, CancellationToken ct) =>
            await svc.DeletePostAsync(id, ct) ? Results.NoContent() : Results.NotFound())
            .WithName("DeletePost").WithTags("WebSpark-Posts");

        // Category CRUD
        group.MapPost("/categories", async (Category entity, WebSparkService svc, CancellationToken ct) =>
        {
            var created = await svc.CreateCategoryAsync(entity, ct);
            return Results.Created($"/api/public/webspark/categories/{created.Id}", created);
        }).WithName("CreateCategory").WithTags("WebSpark-Categories");

        group.MapPut("/categories/{id:int}", async (int id, Category entity, WebSparkService svc, CancellationToken ct) =>
        {
            var updated = await svc.UpdateCategoryAsync(id, entity, ct);
            return updated is null ? Results.NotFound() : Results.Ok(updated);
        }).WithName("UpdateCategory").WithTags("WebSpark-Categories");

        group.MapDelete("/categories/{id:int}", async (int id, WebSparkService svc, CancellationToken ct) =>
            await svc.DeleteCategoryAsync(id, ct) ? Results.NoContent() : Results.NotFound())
            .WithName("DeleteCategory").WithTags("WebSpark-Categories");

        // Menu CRUD
        group.MapPost("/menus", async (Menu entity, WebSparkService svc, CancellationToken ct) =>
        {
            var created = await svc.CreateMenuAsync(entity, ct);
            return Results.Created($"/api/public/webspark/menus/{created.Id}", created);
        }).WithName("CreateMenu").WithTags("WebSpark-Menu");

        group.MapPut("/menus/{id:int}", async (int id, Menu entity, WebSparkService svc, CancellationToken ct) =>
        {
            var updated = await svc.UpdateMenuAsync(id, entity, ct);
            return updated is null ? Results.NotFound() : Results.Ok(updated);
        }).WithName("UpdateMenu").WithTags("WebSpark-Menu");

        group.MapDelete("/menus/{id:int}", async (int id, WebSparkService svc, CancellationToken ct) =>
            await svc.DeleteMenuAsync(id, ct) ? Results.NoContent() : Results.NotFound())
            .WithName("DeleteMenu").WithTags("WebSpark-Menu");

        // Keyword CRUD
        group.MapPost("/keywords", async (Keyword entity, WebSparkService svc, CancellationToken ct) =>
        {
            var created = await svc.CreateKeywordAsync(entity, ct);
            return Results.Created($"/api/public/webspark/keywords/{created.Id}", created);
        }).WithName("CreateKeyword").WithTags("WebSpark-Keywords");

        group.MapPut("/keywords/{id:int}", async (int id, Keyword entity, WebSparkService svc, CancellationToken ct) =>
        {
            var updated = await svc.UpdateKeywordAsync(id, entity, ct);
            return updated is null ? Results.NotFound() : Results.Ok(updated);
        }).WithName("UpdateKeyword").WithTags("WebSpark-Keywords");

        group.MapDelete("/keywords/{id:int}", async (int id, WebSparkService svc, CancellationToken ct) =>
            await svc.DeleteKeywordAsync(id, ct) ? Results.NoContent() : Results.NotFound())
            .WithName("DeleteKeyword").WithTags("WebSpark-Keywords");

        // ContentPart CRUD
        group.MapPost("/content-parts", async (ContentPart entity, WebSparkService svc, CancellationToken ct) =>
        {
            var created = await svc.CreateContentPartAsync(entity, ct);
            return Results.Created($"/api/public/webspark/content-parts/{created.Id}", created);
        }).WithName("CreateContentPart").WithTags("WebSpark-ContentParts");

        group.MapPut("/content-parts/{id:int}", async (int id, ContentPart entity, WebSparkService svc, CancellationToken ct) =>
        {
            var updated = await svc.UpdateContentPartAsync(id, entity, ct);
            return updated is null ? Results.NotFound() : Results.Ok(updated);
        }).WithName("UpdateContentPart").WithTags("WebSpark-ContentParts");

        group.MapDelete("/content-parts/{id:int}", async (int id, WebSparkService svc, CancellationToken ct) =>
            await svc.DeleteContentPartAsync(id, ct) ? Results.NoContent() : Results.NotFound())
            .WithName("DeleteContentPart").WithTags("WebSpark-ContentParts");

        // Subscriber CRUD
        group.MapGet("/subscribers", async (WebSparkService svc, CancellationToken ct) =>
            Results.Ok(await svc.GetSubscribersAsync(ct)))
            .WithName("GetSubscribers").WithTags("WebSpark-Subscribers");

        group.MapGet("/subscribers/{id:int}", async (int id, WebSparkService svc, CancellationToken ct) =>
        {
            var item = await svc.GetSubscriberAsync(id, ct);
            return item is null ? Results.NotFound() : Results.Ok(item);
        }).WithName("GetSubscriber").WithTags("WebSpark-Subscribers");

        group.MapPost("/subscribers", async (Subscriber entity, WebSparkService svc, CancellationToken ct) =>
        {
            var created = await svc.CreateSubscriberAsync(entity, ct);
            return Results.Created($"/api/admin/webspark/subscribers/{created.Id}", created);
        }).WithName("CreateSubscriber").WithTags("WebSpark-Subscribers");

        group.MapPut("/subscribers/{id:int}", async (int id, Subscriber entity, WebSparkService svc, CancellationToken ct) =>
        {
            var updated = await svc.UpdateSubscriberAsync(id, entity, ct);
            return updated is null ? Results.NotFound() : Results.Ok(updated);
        }).WithName("UpdateSubscriber").WithTags("WebSpark-Subscribers");

        group.MapDelete("/subscribers/{id:int}", async (int id, WebSparkService svc, CancellationToken ct) =>
            await svc.DeleteSubscriberAsync(id, ct) ? Results.NoContent() : Results.NotFound())
            .WithName("DeleteSubscriber").WithTags("WebSpark-Subscribers");

        // Newsletter CRUD
        group.MapGet("/newsletters", async (WebSparkService svc, CancellationToken ct) =>
            Results.Ok(await svc.GetNewslettersAsync(ct)))
            .WithName("GetNewsletters").WithTags("WebSpark-Newsletters");

        group.MapGet("/newsletters/{id:int}", async (int id, WebSparkService svc, CancellationToken ct) =>
        {
            var item = await svc.GetNewsletterAsync(id, ct);
            return item is null ? Results.NotFound() : Results.Ok(item);
        }).WithName("GetNewsletter").WithTags("WebSpark-Newsletters");

        group.MapPost("/newsletters", async (Newsletter entity, WebSparkService svc, CancellationToken ct) =>
        {
            var created = await svc.CreateNewsletterAsync(entity, ct);
            return Results.Created($"/api/admin/webspark/newsletters/{created.Id}", created);
        }).WithName("CreateNewsletter").WithTags("WebSpark-Newsletters");

        group.MapDelete("/newsletters/{id:int}", async (int id, WebSparkService svc, CancellationToken ct) =>
            await svc.DeleteNewsletterAsync(id, ct) ? Results.NoContent() : Results.NotFound())
            .WithName("DeleteNewsletter").WithTags("WebSpark-Newsletters");

        // MailSettings CRUD
        group.MapGet("/mail-settings", async (WebSparkService svc, CancellationToken ct) =>
            Results.Ok(await svc.GetMailSettingsAsync(ct)))
            .WithName("GetMailSettings").WithTags("WebSpark-MailSettings");

        group.MapGet("/mail-settings/{id:int}", async (int id, WebSparkService svc, CancellationToken ct) =>
        {
            var item = await svc.GetMailSettingAsync(id, ct);
            return item is null ? Results.NotFound() : Results.Ok(item);
        }).WithName("GetMailSetting").WithTags("WebSpark-MailSettings");

        group.MapPost("/mail-settings", async (MailSetting entity, WebSparkService svc, CancellationToken ct) =>
        {
            var created = await svc.CreateMailSettingAsync(entity, ct);
            return Results.Created($"/api/admin/webspark/mail-settings/{created.Id}", created);
        }).WithName("CreateMailSetting").WithTags("WebSpark-MailSettings");

        group.MapPut("/mail-settings/{id:int}", async (int id, MailSetting entity, WebSparkService svc, CancellationToken ct) =>
        {
            var updated = await svc.UpdateMailSettingAsync(id, entity, ct);
            return updated is null ? Results.NotFound() : Results.Ok(updated);
        }).WithName("UpdateMailSetting").WithTags("WebSpark-MailSettings");

        group.MapDelete("/mail-settings/{id:int}", async (int id, WebSparkService svc, CancellationToken ct) =>
            await svc.DeleteMailSettingAsync(id, ct) ? Results.NoContent() : Results.NotFound())
            .WithName("DeleteMailSetting").WithTags("WebSpark-MailSettings");

        return group;
    }
}
