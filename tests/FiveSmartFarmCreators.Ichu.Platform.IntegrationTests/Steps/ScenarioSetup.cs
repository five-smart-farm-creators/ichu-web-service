using FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Harness;

namespace FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Steps;

/// <summary>
///     Builds the ranches, cattle and devices from which the feature files start, reusing the harness helpers.
/// </summary>
public static class ScenarioSetup
{
    // Milking cow, the life stage that the fever threshold of the scenarios is configured for.
    private const int MonitoredLifeStage = 3;

    /// <summary>
    ///     Registers a verified ranch manager with a ranch, and makes the manager the acting account of the scenario.
    /// </summary>
    public static async Task RegisterRanchManagerAsync(ApiScenarioContext context)
    {
        context.SignIn("manager", TestIdentity.Create());
        var client = context.Actor("manager");
        context.Remember("ranch", await client.RegisterManagerAsync());
        context.Remember("manager account", await client.GetCurrentAccountIdAsync());
    }

    /// <summary>
    ///     Builds cattle wearing an assigned collar, reported through an edge gateway that holds a valid key.
    /// </summary>
    public static async Task MonitorCattleAsync(ApiScenarioContext context)
    {
        var client = context.Actor("manager");
        var ranchId = context.Id("ranch");
        await Arrange.ActivateSubscriptionAsync(context.Factory, client, ranchId, Arrange.StandardPlanId);
        var cattleId = (await Arrange.RegisterCattleAsync(client, ranchId, "PE-0200", MonitoredLifeStage)).GetProperty("id").GetGuid();
        var collar = await Arrange.RegisterCollarAsync(client, ranchId, $"COL-{Guid.NewGuid():N}");
        var collarId = collar.GetProperty("id").GetGuid();
        var assigned = await Arrange.AssignCollarAsync(client, ranchId, collarId, cattleId, collar.GetProperty("version").GetGuid());
        var gateway = await Arrange.RegisterGatewayAsync(client, ranchId, "Corral gateway");

        context.Remember("monitored cattle", cattleId);
        context.Remember("monitored collar", collarId);
        context.RememberVersion("monitored collar", assigned.GetProperty("version").GetGuid());
        context.RememberGateway(gateway.GetProperty("id").GetGuid(),
            gateway.GetProperty("key").GetString() ?? throw new InvalidOperationException("The gateway response has no key."));
    }

    /// <summary>
    ///     Registers a collar in another ranch, whose edge gateway must not be able to report it.
    /// </summary>
    public static async Task<Guid> RegisterCollarInAnotherRanchAsync(ApiScenarioContext context)
    {
        using var client = context.Factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        var collar = await Arrange.RegisterCollarAsync(client, ranchId, $"COL-{Guid.NewGuid():N}");
        return collar.GetProperty("id").GetGuid();
    }
}
