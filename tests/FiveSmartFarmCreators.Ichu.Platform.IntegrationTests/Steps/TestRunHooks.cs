using FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Harness;
using Reqnroll;

namespace FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Steps;

/// <summary>
///     Hosts one API for all BDD scenarios of a test run, so the database is created and dropped once per run.
/// </summary>
[Binding]
public static class TestRunHooks
{
    public static IchuApiFactory Factory { get; private set; } = null!;

    [BeforeTestRun]
    public static void StartApi() => Factory = new IchuApiFactory();

    [AfterTestRun]
    public static async Task StopApiAsync() => await Factory.DisposeAsync();
}
