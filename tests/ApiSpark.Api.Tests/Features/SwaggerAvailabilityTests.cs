using System.Net;
using ApiSpark.Api.Tests.Infrastructure;
using Microsoft.AspNetCore.Hosting;

namespace ApiSpark.Api.Tests.Features;

public class ApiDocsAvailabilityTests
{
    [Fact]
    public async Task ScalarUi_InDevelopment_ReturnsOk()
    {
        await using var factory = new ApiSparkWebApplicationFactory();
        await factory.InitializeAsync();
        var client = factory.CreateClient();
        var response = await client.GetAsync("/scalar/v1");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task OpenApiJson_InDevelopment_ReturnsOk()
    {
        await using var factory = new ApiSparkWebApplicationFactory();
        await factory.InitializeAsync();
        var client = factory.CreateClient();
        var response = await client.GetAsync("/openapi/v1.json");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ScalarUi_InProduction_ReturnsNotFound()
    {
        await using var factory = new ProductionWebApplicationFactory();
        await factory.InitializeAsync();
        var client = factory.CreateClient();
        var response = await client.GetAsync("/scalar/v1");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task OpenApiJson_InProduction_ReturnsNotFound()
    {
        await using var factory = new ProductionWebApplicationFactory();
        await factory.InitializeAsync();
        var client = factory.CreateClient();
        var response = await client.GetAsync("/openapi/v1.json");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

internal class ProductionWebApplicationFactory : ApiSparkWebApplicationFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.UseEnvironment("Production");
    }
}
