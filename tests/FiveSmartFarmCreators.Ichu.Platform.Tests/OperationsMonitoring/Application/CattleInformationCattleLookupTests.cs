using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Acl;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using CattleInformationSnapshot = FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Acl.CattleSnapshot;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.OperationsMonitoring.Application;

public class CattleInformationCattleLookupTests
{
    private static readonly Guid CattleId = Guid.Parse("7b2e3d1f-1e4c-4d62-8b88-2c3d4e5f6a71");
    private static readonly Guid RanchId = Guid.Parse("11111111-1111-4111-8111-111111111111");
    private static readonly Guid LotId = Guid.Parse("6a1f2c0e-0d3b-4c51-9a77-1b2c3d4e5f60");

    private readonly FakeCattleInformationFacade _facade = new();

    [Fact]
    public async Task FindAsync_StartsTheHerdPeriodAtTheStartOfTheBirthDate()
    {
        _facade.Lifecycle = new CattleLifecycleSnapshot(CattleId, RanchId, new DateOnly(2026, 1, 1), null);

        var snapshot = await new CattleInformationCattleLookup(_facade).FindAsync(CattleId, CancellationToken.None);

        Assert.Equal(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), snapshot!.IncorporatedAt);
        Assert.Null(snapshot.DeactivatedAt);
    }

    [Fact]
    public async Task FindAsync_KeepsTheAnimalInTheHerdThroughTheWholeExitDay()
    {
        _facade.Lifecycle = new CattleLifecycleSnapshot(CattleId, RanchId, new DateOnly(2026, 1, 1), new DateOnly(2026, 11, 7));

        var snapshot = await new CattleInformationCattleLookup(_facade).FindAsync(CattleId, CancellationToken.None);

        Assert.True(snapshot!.WasActiveAt(new DateTimeOffset(2026, 11, 7, 23, 59, 0, TimeSpan.Zero)));
        Assert.False(snapshot.WasActiveAt(new DateTimeOffset(2026, 11, 8, 0, 0, 0, TimeSpan.Zero)));
    }

    [Fact]
    public async Task FindAsync_ReturnsNullForAnUnknownAnimal()
    {
        var snapshot = await new CattleInformationCattleLookup(_facade).FindAsync(CattleId, CancellationToken.None);

        Assert.Null(snapshot);
    }

    [Fact]
    public async Task FindProfileAsync_MapsTheSexStageAndBirthDateOfTheAnimal()
    {
        _facade.Animal = new CattleInformationSnapshot(CattleId, RanchId, "PE-0100", "Active", "Female", "MilkingCow",
            new DateOnly(2024, 3, 1), null, null, Guid.NewGuid());

        var profile = await new CattleInformationCattleLookup(_facade).FindProfileAsync(RanchId, CattleId, CancellationToken.None);

        Assert.Equal(new CattleProfileSnapshot(CattleId, RanchId, AnimalSex.Female, "MilkingCow", new DateOnly(2024, 3, 1)), profile);
    }

    [Fact]
    public async Task FindProfileAsync_ReturnsNullForAnAnimalTheCallerCannotSee()
    {
        var profile = await new CattleInformationCattleLookup(_facade).FindProfileAsync(RanchId, CattleId, CancellationToken.None);

        Assert.Null(profile);
    }

    [Fact]
    public async Task LotBelongsToRanchAsync_AsksTheFacadeForTheLotOfTheRanch()
    {
        _facade.LotExists = true;

        var belongs = await new CattleInformationCattleLookup(_facade).LotBelongsToRanchAsync(LotId, RanchId, CancellationToken.None);

        Assert.True(belongs);
        Assert.Equal((RanchId, LotId), _facade.LastLotQuery);
    }

    private sealed class FakeCattleInformationFacade : ICattleInformationContextFacade
    {
        public CattleLifecycleSnapshot? Lifecycle { get; set; }

        public CattleInformationSnapshot? Animal { get; set; }

        public bool LotExists { get; set; }

        public (Guid RanchId, Guid LotId)? LastLotQuery { get; private set; }

        public Task<CattleInformationSnapshot?> FindByIdAsync(Guid ranchId, Guid cattleId, CancellationToken cancellationToken) =>
            Task.FromResult(Animal);

        public Task<bool> LotExistsAsync(Guid ranchId, Guid lotId, CancellationToken cancellationToken)
        {
            LastLotQuery = (ranchId, lotId);
            return Task.FromResult(LotExists);
        }

        public Task<IReadOnlyList<CattleInformationSnapshot>> FindActiveByLotAsync(Guid ranchId, Guid lotId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<CattleInformationSnapshot>>([]);

        public Task<CattleLifecycleSnapshot?> FindLifecycleAsync(Guid cattleId, CancellationToken cancellationToken) =>
            Task.FromResult(Lifecycle);

        public Task<IReadOnlyList<HerdMemberSnapshot>> GetHerdAsync(Guid ranchId, CancellationToken cancellationToken) =>
            throw new NotSupportedException("The lookup resolves single animals and lots only.");

        public Task<IReadOnlyList<CattlePlacementSnapshot>> FindActivePlacementsAsync(Guid ranchId, CancellationToken cancellationToken) =>
            throw new NotSupportedException("The lookup resolves single animals and lots only.");
    }
}
