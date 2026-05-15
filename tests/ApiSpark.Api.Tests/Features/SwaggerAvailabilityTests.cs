using System.Net;
using ApiSpark.Api.Tests.Infrastructure;
using Microsoft.AspNetCore.Hosting;

namespace ApiSpark.Api.Tests.Features;

[TestClass]
public class ApiDocsAvailabilityTests
{
    [TestMethod]
    public async Task ScalarUi_InDevelopment_ReturnsOk()
    {
        await using var factory = new ApiSparkWebApplicationFactory();
        await factory.InitializeAsync();
        var client = factory.CreateClient();
        var response = await client.GetAsync("/scalar/v1");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task OpenApiJson_InDevelopment_ReturnsOk()
    {
        await using var factory = new ApiSparkWebApplicationFactory();
        await factory.InitializeAsync();
        var client = factory.CreateClient();
        var response = await client.GetAsync("/openapi/v1.json");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task ScalarUi_InProduction_ReturnsNotFound()
    {
        await using var factory = new ProductionWebApplicationFactory();
        await factory.InitializeAsync();
        var client = factory.CreateClient();
        var response = await client.GetAsync("/scalar/v1");
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task OpenApiJson_InProduction_ReturnsNotFound()
    {
        await using var factory = new ProductionWebApplicationFactory();
        await factory.InitializeAsync();
        var client = factory.CreateClient();
        var response = await client.GetAsync("/openapi/v1.json");
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
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
