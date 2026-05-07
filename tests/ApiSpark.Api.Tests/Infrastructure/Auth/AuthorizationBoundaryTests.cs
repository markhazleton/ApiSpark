using System.Net;
using ApiSpark.Api.Tests.Infrastructure;

namespace ApiSpark.Api.Tests.Infrastructure.Auth;

public class AuthorizationBoundaryTests(ApiSparkWebApplicationFactory factory)
    : IClassFixture<ApiSparkWebApplicationFactory>
{
    [Fact]
    public async Task AdminRoute_WithoutAuth_Returns401()
    {
        var client = factory.CreateClient();
        var response = await client.GetAsync("/api/admin/health/deep");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AdminRoute_WithAdminRole_Returns200()
    {
        var client = factory.CreateAdminClient();
        var response = await client.GetAsync("/api/admin/health/deep");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AdminRoute_WithNonAdminRole_Returns403()
    {
        var client = factory.CreatePublisherClient();
        var response = await client.GetAsync("/api/admin/health/deep");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task PublicRoute_WithoutAuth_Returns200()
    {
        var client = factory.CreateClient();
        var response = await client.GetAsync("/api/public/content/articles");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
