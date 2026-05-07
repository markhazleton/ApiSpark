using System.Net;
using ApiSpark.Api.Tests.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ApiSpark.Api.Tests.Features;

public class SwaggerAvailabilityTests
{
    [Fact]
    public async Task Swagger_InDevelopment_ReturnsOk()
    {
        await using var factory = new ApiSparkWebApplicationFactory();
        await factory.InitializeAsync();
        var client = factory.CreateClient();
        var response = await client.GetAsync("/swagger/index.html");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Swagger_InProduction_ReturnsNotFound()
    {
        await using var factory = new ProductionWebApplicationFactory();
        await factory.InitializeAsync();
        var client = factory.CreateClient();
        var response = await client.GetAsync("/swagger/index.html");
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
