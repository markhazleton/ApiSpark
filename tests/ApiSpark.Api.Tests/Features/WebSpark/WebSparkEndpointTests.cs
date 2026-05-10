using System.Net;
using System.Net.Http.Json;
using ApiSpark.Api.Tests.Infrastructure;

namespace ApiSpark.Api.Tests.Features.WebSpark;

public class WebSparkEndpointTests(ApiSparkWebApplicationFactory factory)
    : IClassFixture<ApiSparkWebApplicationFactory>
{
    private readonly HttpClient _anonClient  = factory.CreateClient();
    private readonly HttpClient _adminClient = factory.CreateAdminClient();

    // ── Public read endpoints (anonymous) ────────────────────────────────────

    [Theory]
    [InlineData("/api/public/webspark/domains")]
    [InlineData("/api/public/webspark/blogs")]
    [InlineData("/api/public/webspark/authors")]
    [InlineData("/api/public/webspark/posts")]
    [InlineData("/api/public/webspark/categories")]
    [InlineData("/api/public/webspark/menus")]
    [InlineData("/api/public/webspark/keywords")]
    [InlineData("/api/public/webspark/content-parts")]
    public async Task PublicWebSparkEndpoints_Anonymous_ReturnOk(string path)
    {
        var response = await _anonClient.GetAsync(path);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/public/webspark/domains")]
    [InlineData("/api/public/webspark/blogs")]
    [InlineData("/api/public/webspark/authors")]
    [InlineData("/api/public/webspark/posts")]
    [InlineData("/api/public/webspark/categories")]
    [InlineData("/api/public/webspark/menus")]
    [InlineData("/api/public/webspark/keywords")]
    [InlineData("/api/public/webspark/content-parts")]
    public async Task PublicWebSparkListEndpoints_ReturnJsonArray(string path)
    {
        var response = await _anonClient.GetAsync(path);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<object[]>();
        Assert.NotNull(body);
    }

    [Theory]
    [InlineData("/api/public/webspark/domains/99999")]
    [InlineData("/api/public/webspark/blogs/99999")]
    [InlineData("/api/public/webspark/authors/99999")]
    [InlineData("/api/public/webspark/posts/99999")]
    [InlineData("/api/public/webspark/categories/99999")]
    [InlineData("/api/public/webspark/menus/99999")]
    [InlineData("/api/public/webspark/keywords/99999")]
    [InlineData("/api/public/webspark/content-parts/99999")]
    public async Task PublicWebSparkDetailEndpoints_NonExistent_ReturnNotFound(string path)
    {
        var response = await _anonClient.GetAsync(path);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── Admin endpoints: unauthenticated → 401 ───────────────────────────────

    [Theory]
    [InlineData("POST",   "/api/admin/webspark/domains")]
    [InlineData("PUT",    "/api/admin/webspark/domains/1")]
    [InlineData("DELETE", "/api/admin/webspark/domains/1")]
    [InlineData("POST",   "/api/admin/webspark/blogs")]
    [InlineData("POST",   "/api/admin/webspark/authors")]
    [InlineData("POST",   "/api/admin/webspark/posts")]
    [InlineData("GET",    "/api/admin/webspark/subscribers")]
    [InlineData("POST",   "/api/admin/webspark/subscribers")]
    [InlineData("GET",    "/api/admin/webspark/newsletters")]
    [InlineData("GET",    "/api/admin/webspark/mail-settings")]
    public async Task AdminWebSparkEndpoints_WithoutAuth_ReturnUnauthorized(string method, string path)
    {
        var request = new HttpRequestMessage(new HttpMethod(method), path);
        if (method is "POST" or "PUT")
            request.Content = JsonContent.Create(new { });

        var response = await _anonClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ── Admin endpoints: authenticated Admin → authorization accepted ─────────

    [Theory]
    [InlineData("/api/admin/webspark/subscribers")]
    [InlineData("/api/admin/webspark/newsletters")]
    [InlineData("/api/admin/webspark/mail-settings")]
    public async Task AdminWebSparkGetEndpoints_WithAdminAuth_ReturnOk(string path)
    {
        var response = await _adminClient.GetAsync(path);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/admin/webspark/domains")]
    [InlineData("/api/admin/webspark/blogs")]
    [InlineData("/api/admin/webspark/authors")]
    [InlineData("/api/admin/webspark/posts")]
    [InlineData("/api/admin/webspark/categories")]
    [InlineData("/api/admin/webspark/menus")]
    [InlineData("/api/admin/webspark/keywords")]
    [InlineData("/api/admin/webspark/content-parts")]
    public async Task AdminWebSparkCreateEndpoints_WithAdminAuth_IsAuthorizationAccepted(string path)
    {
        // Verifies Admin role is accepted by the authorization layer.
        // The request may fail with 400/422 due to missing required fields in the body —
        // that is a data concern, not an authorization concern. We assert it is NOT 401 or 403.
        var response = await _adminClient.PostAsJsonAsync(path, new { });
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
