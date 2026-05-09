using Microsoft.EntityFrameworkCore;
using WebSpark.Core.Data;

namespace ApiSpark.Api.Features.WebSpark;

public class WebSparkService(WebSparkDbContext db)
{
    // ── Domain (WebSite) ────────────────────────────────────────────────────

    public async Task<List<WebSite>> GetDomainsAsync(CancellationToken ct = default)
        => await db.Domain.OrderBy(d => d.Name).ToListAsync(ct);

    public async Task<WebSite?> GetDomainAsync(int id, CancellationToken ct = default)
        => await db.Domain.FindAsync([id], ct);

    public async Task<WebSite> CreateDomainAsync(WebSite entity, CancellationToken ct = default)
    {
        db.Domain.Add(entity);
        await db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<WebSite?> UpdateDomainAsync(int id, WebSite entity, CancellationToken ct = default)
    {
        var existing = await db.Domain.FindAsync([id], ct);
        if (existing is null) return null;
        entity.Id = id;
        db.Entry(existing).CurrentValues.SetValues(entity);
        await db.SaveChangesAsync(ct);
        return existing;
    }

    public async Task<bool> DeleteDomainAsync(int id, CancellationToken ct = default)
    {
        var existing = await db.Domain.FindAsync([id], ct);
        if (existing is null) return false;
        db.Domain.Remove(existing);
        await db.SaveChangesAsync(ct);
        return true;
    }

    // ── Blogs ───────────────────────────────────────────────────────────────

    public async Task<List<Blog>> GetBlogsAsync(CancellationToken ct = default)
        => await db.Blogs.OrderBy(b => b.Title).ToListAsync(ct);

    public async Task<Blog?> GetBlogAsync(int id, CancellationToken ct = default)
        => await db.Blogs.FindAsync([id], ct);

    public async Task<Blog> CreateBlogAsync(Blog entity, CancellationToken ct = default)
    {
        db.Blogs.Add(entity);
        await db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<Blog?> UpdateBlogAsync(int id, Blog entity, CancellationToken ct = default)
    {
        var existing = await db.Blogs.FindAsync([id], ct);
        if (existing is null) return null;
        entity.Id = id;
        db.Entry(existing).CurrentValues.SetValues(entity);
        await db.SaveChangesAsync(ct);
        return existing;
    }

    public async Task<bool> DeleteBlogAsync(int id, CancellationToken ct = default)
    {
        var existing = await db.Blogs.FindAsync([id], ct);
        if (existing is null) return false;
        db.Blogs.Remove(existing);
        await db.SaveChangesAsync(ct);
        return true;
    }

    // ── Authors ─────────────────────────────────────────────────────────────

    public async Task<List<Author>> GetAuthorsAsync(CancellationToken ct = default)
        => await db.Authors.OrderBy(a => a.DisplayName).ToListAsync(ct);

    public async Task<Author?> GetAuthorAsync(int id, CancellationToken ct = default)
        => await db.Authors.FindAsync([id], ct);

    public async Task<Author> CreateAuthorAsync(Author entity, CancellationToken ct = default)
    {
        db.Authors.Add(entity);
        await db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<Author?> UpdateAuthorAsync(int id, Author entity, CancellationToken ct = default)
    {
        var existing = await db.Authors.FindAsync([id], ct);
        if (existing is null) return null;
        entity.Id = id;
        db.Entry(existing).CurrentValues.SetValues(entity);
        await db.SaveChangesAsync(ct);
        return existing;
    }

    public async Task<bool> DeleteAuthorAsync(int id, CancellationToken ct = default)
    {
        var existing = await db.Authors.FindAsync([id], ct);
        if (existing is null) return false;
        db.Authors.Remove(existing);
        await db.SaveChangesAsync(ct);
        return true;
    }

    // ── Posts ────────────────────────────────────────────────────────────────

    public async Task<List<Post>> GetPostsAsync(int? blogId, CancellationToken ct = default)
    {
        var query = db.Posts.AsQueryable();
        if (blogId.HasValue) query = query.Where(p => p.Blog.Id == blogId.Value);
        return await query.OrderByDescending(p => p.Published).ToListAsync(ct);
    }

    public async Task<Post?> GetPostAsync(int id, CancellationToken ct = default)
        => await db.Posts.FindAsync([id], ct);

    public async Task<Post> CreatePostAsync(Post entity, CancellationToken ct = default)
    {
        db.Posts.Add(entity);
        await db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<Post?> UpdatePostAsync(int id, Post entity, CancellationToken ct = default)
    {
        var existing = await db.Posts.FindAsync([id], ct);
        if (existing is null) return null;
        entity.Id = id;
        db.Entry(existing).CurrentValues.SetValues(entity);
        await db.SaveChangesAsync(ct);
        return existing;
    }

    public async Task<bool> DeletePostAsync(int id, CancellationToken ct = default)
    {
        var existing = await db.Posts.FindAsync([id], ct);
        if (existing is null) return false;
        db.Posts.Remove(existing);
        await db.SaveChangesAsync(ct);
        return true;
    }

    // ── Categories ──────────────────────────────────────────────────────────

    public async Task<List<Category>> GetCategoriesAsync(CancellationToken ct = default)
        => await db.Categories.OrderBy(c => c.Content).ToListAsync(ct);

    public async Task<Category?> GetCategoryAsync(int id, CancellationToken ct = default)
        => await db.Categories.FindAsync([id], ct);

    public async Task<Category> CreateCategoryAsync(Category entity, CancellationToken ct = default)
    {
        db.Categories.Add(entity);
        await db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<Category?> UpdateCategoryAsync(int id, Category entity, CancellationToken ct = default)
    {
        var existing = await db.Categories.FindAsync([id], ct);
        if (existing is null) return null;
        entity.Id = id;
        db.Entry(existing).CurrentValues.SetValues(entity);
        await db.SaveChangesAsync(ct);
        return existing;
    }

    public async Task<bool> DeleteCategoryAsync(int id, CancellationToken ct = default)
    {
        var existing = await db.Categories.FindAsync([id], ct);
        if (existing is null) return false;
        db.Categories.Remove(existing);
        await db.SaveChangesAsync(ct);
        return true;
    }

    // ── Menu ────────────────────────────────────────────────────────────────

    public async Task<List<Menu>> GetMenusAsync(int? domainId, CancellationToken ct = default)
    {
        var query = db.Menu.AsQueryable();
        if (domainId.HasValue) query = query.Where(m => m.Domain.Id == domainId.Value);
        return await query.OrderBy(m => m.DisplayOrder).ThenBy(m => m.Title).ToListAsync(ct);
    }

    public async Task<Menu?> GetMenuAsync(int id, CancellationToken ct = default)
        => await db.Menu.FindAsync([id], ct);

    public async Task<Menu> CreateMenuAsync(Menu entity, CancellationToken ct = default)
    {
        db.Menu.Add(entity);
        await db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<Menu?> UpdateMenuAsync(int id, Menu entity, CancellationToken ct = default)
    {
        var existing = await db.Menu.FindAsync([id], ct);
        if (existing is null) return null;
        entity.Id = id;
        db.Entry(existing).CurrentValues.SetValues(entity);
        await db.SaveChangesAsync(ct);
        return existing;
    }

    public async Task<bool> DeleteMenuAsync(int id, CancellationToken ct = default)
    {
        var existing = await db.Menu.FindAsync([id], ct);
        if (existing is null) return false;
        db.Menu.Remove(existing);
        await db.SaveChangesAsync(ct);
        return true;
    }

    // ── Keywords ────────────────────────────────────────────────────────────

    public async Task<List<Keyword>> GetKeywordsAsync(CancellationToken ct = default)
        => await db.Keywords.OrderBy(k => k.Name).ToListAsync(ct);

    public async Task<Keyword?> GetKeywordAsync(int id, CancellationToken ct = default)
        => await db.Keywords.FindAsync([id], ct);

    public async Task<Keyword> CreateKeywordAsync(Keyword entity, CancellationToken ct = default)
    {
        db.Keywords.Add(entity);
        await db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<Keyword?> UpdateKeywordAsync(int id, Keyword entity, CancellationToken ct = default)
    {
        var existing = await db.Keywords.FindAsync([id], ct);
        if (existing is null) return null;
        entity.Id = id;
        db.Entry(existing).CurrentValues.SetValues(entity);
        await db.SaveChangesAsync(ct);
        return existing;
    }

    public async Task<bool> DeleteKeywordAsync(int id, CancellationToken ct = default)
    {
        var existing = await db.Keywords.FindAsync([id], ct);
        if (existing is null) return false;
        db.Keywords.Remove(existing);
        await db.SaveChangesAsync(ct);
        return true;
    }

    // ── ContentParts ────────────────────────────────────────────────────────

    public async Task<List<ContentPart>> GetContentPartsAsync(CancellationToken ct = default)
        => await db.ContentParts.OrderBy(c => c.Title).ToListAsync(ct);

    public async Task<ContentPart?> GetContentPartAsync(int id, CancellationToken ct = default)
        => await db.ContentParts.FindAsync([id], ct);

    public async Task<ContentPart> CreateContentPartAsync(ContentPart entity, CancellationToken ct = default)
    {
        db.ContentParts.Add(entity);
        await db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<ContentPart?> UpdateContentPartAsync(int id, ContentPart entity, CancellationToken ct = default)
    {
        var existing = await db.ContentParts.FindAsync([id], ct);
        if (existing is null) return null;
        entity.Id = id;
        db.Entry(existing).CurrentValues.SetValues(entity);
        await db.SaveChangesAsync(ct);
        return existing;
    }

    public async Task<bool> DeleteContentPartAsync(int id, CancellationToken ct = default)
    {
        var existing = await db.ContentParts.FindAsync([id], ct);
        if (existing is null) return false;
        db.ContentParts.Remove(existing);
        await db.SaveChangesAsync(ct);
        return true;
    }

    // ── Subscribers ─────────────────────────────────────────────────────────

    public async Task<List<Subscriber>> GetSubscribersAsync(CancellationToken ct = default)
        => await db.Subscribers.OrderBy(s => s.Email).ToListAsync(ct);

    public async Task<Subscriber?> GetSubscriberAsync(int id, CancellationToken ct = default)
        => await db.Subscribers.FindAsync([id], ct);

    public async Task<Subscriber> CreateSubscriberAsync(Subscriber entity, CancellationToken ct = default)
    {
        db.Subscribers.Add(entity);
        await db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<Subscriber?> UpdateSubscriberAsync(int id, Subscriber entity, CancellationToken ct = default)
    {
        var existing = await db.Subscribers.FindAsync([id], ct);
        if (existing is null) return null;
        entity.Id = id;
        db.Entry(existing).CurrentValues.SetValues(entity);
        await db.SaveChangesAsync(ct);
        return existing;
    }

    public async Task<bool> DeleteSubscriberAsync(int id, CancellationToken ct = default)
    {
        var existing = await db.Subscribers.FindAsync([id], ct);
        if (existing is null) return false;
        db.Subscribers.Remove(existing);
        await db.SaveChangesAsync(ct);
        return true;
    }

    // ── Newsletters ─────────────────────────────────────────────────────────

    public async Task<List<Newsletter>> GetNewslettersAsync(CancellationToken ct = default)
        => await db.Newsletters.OrderByDescending(n => n.CreatedDate).ToListAsync(ct);

    public async Task<Newsletter?> GetNewsletterAsync(int id, CancellationToken ct = default)
        => await db.Newsletters.FindAsync([id], ct);

    public async Task<Newsletter> CreateNewsletterAsync(Newsletter entity, CancellationToken ct = default)
    {
        db.Newsletters.Add(entity);
        await db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<bool> DeleteNewsletterAsync(int id, CancellationToken ct = default)
    {
        var existing = await db.Newsletters.FindAsync([id], ct);
        if (existing is null) return false;
        db.Newsletters.Remove(existing);
        await db.SaveChangesAsync(ct);
        return true;
    }

    // ── MailSettings ─────────────────────────────────────────────────────────

    public async Task<List<MailSetting>> GetMailSettingsAsync(CancellationToken ct = default)
        => await db.MailSettings.OrderBy(m => m.FromEmail).ToListAsync(ct);

    public async Task<MailSetting?> GetMailSettingAsync(int id, CancellationToken ct = default)
        => await db.MailSettings.FindAsync([id], ct);

    public async Task<MailSetting> CreateMailSettingAsync(MailSetting entity, CancellationToken ct = default)
    {
        db.MailSettings.Add(entity);
        await db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<MailSetting?> UpdateMailSettingAsync(int id, MailSetting entity, CancellationToken ct = default)
    {
        var existing = await db.MailSettings.FindAsync([id], ct);
        if (existing is null) return null;
        entity.Id = id;
        db.Entry(existing).CurrentValues.SetValues(entity);
        await db.SaveChangesAsync(ct);
        return existing;
    }

    public async Task<bool> DeleteMailSettingAsync(int id, CancellationToken ct = default)
    {
        var existing = await db.MailSettings.FindAsync([id], ct);
        if (existing is null) return false;
        db.MailSettings.Remove(existing);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
