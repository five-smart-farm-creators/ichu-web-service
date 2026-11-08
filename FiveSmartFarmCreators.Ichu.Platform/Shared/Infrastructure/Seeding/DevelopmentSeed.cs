namespace FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Seeding;

/// <summary>
///     Fixed identifiers of the Development data, shared by the seeders of several contexts. They match the edge
///     service's development seed, so the edge keeps working against a freshly seeded database.
/// </summary>
public static class DevelopmentSeed
{
    public static readonly Guid RanchId = Guid.Parse("11111111-1111-4111-8111-111111111111");
    public static readonly Guid LotId = Guid.Parse("6a1f2c0e-0d3b-4c51-9a77-1b2c3d4e5f60");
    public static readonly Guid MilkingCowId = Guid.Parse("7b2e3d1f-1e4c-4d62-8b88-2c3d4e5f6a71");
    public static readonly Guid HeiferId = Guid.Parse("8c3f4e2a-2f5d-4e73-9c99-3d4e5f6a7b82");
    public static readonly Guid GatewayId = Guid.Parse("c07d8c6e-6d9b-4cb7-9add-7b8c9daebfc6");
    public const string GatewaySecret = "development-gateway-key";
}
