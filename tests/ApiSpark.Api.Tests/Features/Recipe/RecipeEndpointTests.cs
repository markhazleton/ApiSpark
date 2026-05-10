using System.Net;
using System.Net.Http.Json;
using ApiSpark.Api.Tests.Infrastructure;

namespace ApiSpark.Api.Tests.Features.Recipe;

public class RecipeEndpointTests(ApiSparkWebApplicationFactory factory)
    : IClassFixture<ApiSparkWebApplicationFactory>
{
    private readonly HttpClient _anonClient      = factory.CreateClient();
    private readonly HttpClient _publisherClient = factory.CreatePublisherClient();

    // ── Public read endpoints (anonymous) ────────────────────────────────────

    [Fact]
    public async Task GetRecipes_Anonymous_ReturnsOk()
    {
        var response = await _anonClient.GetAsync("/api/public/recipes");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetRecipes_Anonymous_ReturnsJsonArray()
    {
        var response = await _anonClient.GetAsync("/api/public/recipes");
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<object[]>();
        Assert.NotNull(body);
    }

    [Fact]
    public async Task GetRecipeCategories_Anonymous_ReturnsOk()
    {
        var response = await _anonClient.GetAsync("/api/public/recipes/categories");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetRecipeById_NonExistent_ReturnsNotFound()
    {
        var response = await _anonClient.GetAsync("/api/public/recipes/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── Publish endpoints: authorization boundary ─────────────────────────────

    [Fact]
    public async Task CreateRecipe_WithoutAuth_ReturnsUnauthorized()
    {
        var payload = new { name = "Test Recipe", ingredients = "x", instructions = "y", authorName = "Test", recipeCategoryId = 1 };
        var response = await _anonClient.PostAsJsonAsync("/api/publish/recipes", payload);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateRecipe_WithoutAuth_ReturnsUnauthorized()
    {
        var payload = new { name = "Test Recipe", ingredients = "x", instructions = "y", authorName = "Test", recipeCategoryId = 1 };
        var response = await _anonClient.PutAsJsonAsync("/api/publish/recipes/1", payload);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteRecipe_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _anonClient.DeleteAsync("/api/publish/recipes/1");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateRecipeCategory_WithoutAuth_ReturnsUnauthorized()
    {
        var payload = new { name = "Test Category" };
        var response = await _anonClient.PostAsJsonAsync("/api/publish/recipes/categories", payload);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateRecipe_WithPublisherAuth_IsAuthorizationAccepted()
    {
        // Verifies Publisher role is accepted by the authorization layer (auth check passes).
        // The request may fail with 400/500 if DB has no matching category — that is a data
        // concern, not an authorization concern. We assert it is NOT 401 or 403.
        var payload = new { name = "Test Recipe", ingredients = "x", instructions = "y", authorName = "Test", recipeCategoryId = 1 };
        var response = await _publisherClient.PostAsJsonAsync("/api/publish/recipes", payload);
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteRecipe_WithPublisherAuth_NonExistent_IsAuthorizationAccepted()
    {
        // Verifies Publisher role is accepted — returns NotFound (not 401/403).
        var response = await _publisherClient.DeleteAsync("/api/publish/recipes/99999");
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
