using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Harness;

/// <summary>
///     Builds authenticated HTTP clients and checks responses for the integration tests.
/// </summary>
public static class ApiClient
{
    private const string GatewayKeyHeader = "X-Gateway-Key";

    public static HttpClient CreateClient(this IchuApiFactory factory, TestIdentity? identity = null)
    {
        var client = factory.CreateClient();
        if (identity is not null)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", identity.BearerToken);
        }

        return client;
    }

    /// <summary>
    ///     Formats an instant for a query string in the round-trip form that the API binds to DateTimeOffset.
    /// </summary>
    public static string ToQueryValue(this DateTimeOffset value) =>
        Uri.EscapeDataString(value.ToString("O", System.Globalization.CultureInfo.InvariantCulture));

    public static Task<HttpResponseMessage> PostJsonAsync(this HttpClient client, string requestUri, object body) =>
        client.PostAsJsonAsync(requestUri, body);

    public static Task<HttpResponseMessage> PutJsonAsync(this HttpClient client, string requestUri, object body) =>
        client.PutAsJsonAsync(requestUri, body);

    public static Task<HttpResponseMessage> PatchJsonAsync(this HttpClient client, string requestUri, object body) =>
        client.PatchAsJsonAsync(requestUri, body);

    public static async Task<HttpResponseMessage> GetWithGatewayKeyAsync(this HttpClient client, string requestUri, string gatewayKey)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        request.Headers.Add(GatewayKeyHeader, gatewayKey);
        return await client.SendAsync(request);
    }

    public static async Task<HttpResponseMessage> PostJsonWithGatewayKeyAsync(this HttpClient client, string requestUri, string gatewayKey, object body)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri) { Content = JsonContent.Create(body) };
        request.Headers.Add(GatewayKeyHeader, gatewayKey);
        return await client.SendAsync(request);
    }

    /// <summary>
    ///     Asserts the status code and returns the body, so a failure shows what the API answered.
    /// </summary>
    public static async Task<string> ReadBodyAsync(this HttpResponseMessage response, HttpStatusCode expectedStatus)
    {
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == expectedStatus,
            $"Expected HTTP {(int)expectedStatus} but received {(int)response.StatusCode}: {body}");
        return body;
    }

    public static async Task<JsonElement> ReadJsonAsync(this HttpResponseMessage response, HttpStatusCode expectedStatus) =>
        JsonSerializer.Deserialize<JsonElement>(await response.ReadBodyAsync(expectedStatus));

    /// <summary>
    ///     Registers the caller as a ranch manager with a new ranch and returns the ranch identifier.
    /// </summary>
    public static async Task<Guid> RegisterManagerAsync(this HttpClient client)
    {
        using var response = await client.PostJsonAsync("api/v1/accounts", new
        {
            role = "RanchManager",
            ranchName = "Los Andes",
            region = "Cusco",
            district = "Urubamba",
            estimatedHeadCount = 120
        });
        var body = await response.ReadJsonAsync(HttpStatusCode.Created);
        return body.GetProperty("ranchId").GetGuid();
    }

    public static async Task<Guid> GetCurrentAccountIdAsync(this HttpClient client)
    {
        using var response = await client.GetAsync("api/v1/accounts/me");
        var body = await response.ReadJsonAsync(HttpStatusCode.OK);
        return body.GetProperty("id").GetGuid();
    }
}
