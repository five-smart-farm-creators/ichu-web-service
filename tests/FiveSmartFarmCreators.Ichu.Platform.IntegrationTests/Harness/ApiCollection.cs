namespace FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Harness;

/// <summary>
///     Shares one hosted API and one database across all integration test classes of a run.
/// </summary>
[CollectionDefinition(ApiCollection.Name)]
public sealed class ApiCollection : ICollectionFixture<IchuApiFactory>
{
    public const string Name = "api";
}
