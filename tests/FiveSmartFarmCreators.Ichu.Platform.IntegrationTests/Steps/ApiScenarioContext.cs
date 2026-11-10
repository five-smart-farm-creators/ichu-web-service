using System.Net;
using System.Text.Json;
using FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Harness;
using Reqnroll;

namespace FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Steps;

/// <summary>
///     Shares the actors, aliases, pending edge records and last response among the steps of one scenario.
/// </summary>
[Binding]
public sealed class ApiScenarioContext : IDisposable
{
    private readonly Dictionary<string, HttpClient> _actors = new(StringComparer.Ordinal);
    private readonly List<HttpClient> _anonymousClients = [];
    private readonly Dictionary<string, Guid> _ids = new(StringComparer.Ordinal);
    private readonly Dictionary<string, DateTimeOffset> _times = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Guid> _versions = new(StringComparer.Ordinal);
    private JsonElement? _lastBody;
    private string? _gatewayKey;

    public ApiScenarioContext()
    {
        // Gherkin tables refer to the seeded plans by name, such as <Standard>.
        Remember("Basic", Arrange.BasicPlanId);
        Remember("Standard", Arrange.StandardPlanId);
        Remember("Pro", Arrange.ProPlanId);
    }

    public IchuApiFactory Factory => TestRunHooks.Factory;

    public PendingEdgeRecords PendingRecords { get; } = new();

    public object? LastBatch { get; set; }

    /// <summary>
    ///     The highest rule revision published in the scenario; an alert may cite a revision no newer than it.
    /// </summary>
    public long RuleRevision { get; set; }

    public HttpStatusCode LastStatus { get; private set; }

    public string LastResponseText { get; private set; } = string.Empty;

    public string GatewayKey => _gatewayKey ?? throw new InvalidOperationException("The scenario has no edge gateway.");

    public void RememberGateway(Guid gatewayId, string key)
    {
        Remember("gateway", gatewayId);
        _gatewayKey = key;
    }

    public void SignIn(string actor, TestIdentity identity)
    {
        if (_actors.Remove(actor, out var previous))
        {
            previous.Dispose();
        }

        _actors[actor] = Factory.CreateClient(identity);
    }

    public HttpClient Actor(string actor) =>
        _actors.TryGetValue(actor, out var client) ? client : throw new InvalidOperationException($"The actor '{actor}' has not signed in.");

    public HttpClient Anonymous()
    {
        var client = Factory.CreateClient();
        _anonymousClients.Add(client);
        return client;
    }

    public void Remember(string alias, Guid id) => _ids[alias] = id;

    public Guid Id(string alias) =>
        _ids.TryGetValue(alias, out var id) ? id : throw new InvalidOperationException($"No record is known as '{alias}'.");

    public void RememberTime(string alias, DateTimeOffset instant) => _times[alias] = instant;

    public DateTimeOffset Time(string alias) =>
        _times.TryGetValue(alias, out var instant) ? instant : throw new InvalidOperationException($"No time is known as '{alias}'.");

    public void RememberVersion(string alias, Guid version) => _versions[alias] = version;

    public Guid Version(string alias) =>
        _versions.TryGetValue(alias, out var version) ? version : throw new InvalidOperationException($"No version is known for '{alias}'.");

    /// <summary>
    ///     Keeps the status and the parsed body of a response so that the assertion steps can check them.
    /// </summary>
    public async Task RecordAsync(Task<HttpResponseMessage> request)
    {
        using var response = await request;
        var text = await response.Content.ReadAsStringAsync();
        LastStatus = response.StatusCode;
        LastResponseText = text;
        _lastBody = text.Length == 0 ? null : JsonSerializer.Deserialize<JsonElement>(text);
    }

    public JsonElement RequireBody() =>
        _lastBody ?? throw new InvalidOperationException("The last request returned no body.");

    /// <summary>
    ///     Builds the query of the last days up to one hour from now, so that every capture of a scenario falls inside it.
    /// </summary>
    public static string LastDaysQuery(int days) =>
        $"from={DateTimeOffset.UtcNow.AddDays(-days).ToQueryValue()}&to={DateTimeOffset.UtcNow.AddHours(1).ToQueryValue()}";

    public void Dispose()
    {
        foreach (var client in _actors.Values)
        {
            client.Dispose();
        }

        foreach (var client in _anonymousClients)
        {
            client.Dispose();
        }
    }
}
