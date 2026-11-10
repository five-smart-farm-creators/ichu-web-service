using System.Net;
using FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Harness;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Shared;

/// <summary>
///     Covers the hosting endpoints that Render relies on: the health probe, the optional OpenAPI document and the
///     HTTPS scheme that the proxy forwards.
/// </summary>
[Collection(ApiCollection.Name)]
public sealed class HostingApiTests(IchuApiFactory factory)
{
    [Fact]
    public async Task HealthEndpoint_WithoutToken_ReturnsHealthy()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("health");

        Assert.Equal("Healthy", await response.ReadBodyAsync(HttpStatusCode.OK));
    }

    [Fact]
    public async Task OpenApiDocument_ByDefault_IsNotFound()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("openapi/v1.json");

        await response.ReadBodyAsync(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task OpenApiDocument_WhenEnabledByConfiguration_DeclaresBearerScheme()
    {
        using var host = factory.WithWebHostBuilder(builder => builder.ConfigureAppConfiguration(configuration =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["OpenApi:Enabled"] = "true" })));
        using var client = host.CreateClient();

        using var response = await client.GetAsync("openapi/v1.json");

        var document = await response.ReadJsonAsync(HttpStatusCode.OK);
        var bearer = document.GetProperty("components").GetProperty("securitySchemes").GetProperty("Bearer");
        Assert.Equal("http", bearer.GetProperty("type").GetString());
        Assert.Equal("bearer", bearer.GetProperty("scheme").GetString());
    }

    [Fact]
    public async Task ForwardedHttpsScheme_PreventsRedirectToHttps()
    {
        // Redirects need a known HTTPS port, and the client must not follow them, so the redirect itself can be observed.
        using var host = factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
            services.Configure<HttpsRedirectionOptions>(options => options.HttpsPort = 443)));
        using var client = host.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        using var plainResponse = await client.GetAsync("health");
        Assert.Equal(HttpStatusCode.TemporaryRedirect, plainResponse.StatusCode);
        Assert.Equal("https", plainResponse.Headers.Location?.Scheme);

        using var forwardedRequest = new HttpRequestMessage(HttpMethod.Get, "health");
        forwardedRequest.Headers.Add("X-Forwarded-Proto", "https");
        using var forwardedResponse = await client.SendAsync(forwardedRequest);
        Assert.Equal("Healthy", await forwardedResponse.ReadBodyAsync(HttpStatusCode.OK));
    }
}
