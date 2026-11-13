using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Acl;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Internal.OutboundServices;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.Planning.Application;

public class CattleInformationPlanningCattleLookupTests
{
    private static readonly Guid RanchId = Guid.Parse("11111111-1111-4111-8111-111111111111");
    private static readonly Guid CattleId = Guid.Parse("33333333-3333-4333-8333-333333333333");

    [Fact]
    public async Task CattleBelongsToRanch_IsTrueWhenCattleInformationReturnsTheAnimal()
    {
        var facade = new FakeCattleInformationFacade
        {
            Animal = new CattleSnapshot(CattleId, RanchId, "A-1", "Active", "Female", "Calf", new DateOnly(2026, 1, 1), null, null, Guid.NewGuid())
        };

        var belongs = await new CattleInformationPlanningCattleLookup(facade).CattleBelongsToRanchAsync(RanchId, CattleId, CancellationToken.None);

        Assert.True(belongs);
        Assert.Equal((RanchId, CattleId), facade.LastQuery);
    }

    [Fact]
    public async Task CattleBelongsToRanch_IsFalseWhenCattleInformationHasNoVisibleAnimal()
    {
        var belongs = await new CattleInformationPlanningCattleLookup(new FakeCattleInformationFacade())
            .CattleBelongsToRanchAsync(RanchId, CattleId, CancellationToken.None);

        Assert.False(belongs);
    }

    private sealed class FakeCattleInformationFacade : ICattleInformationContextFacade
    {
        public CattleSnapshot? Animal { get; init; }

        public (Guid RanchId, Guid CattleId)? LastQuery { get; private set; }

        public Task<CattleSnapshot?> FindByIdAsync(Guid ranchId, Guid cattleId, CancellationToken cancellationToken)
        {
            LastQuery = (ranchId, cattleId);
            return Task.FromResult(Animal);
        }

        public Task<bool> LotExistsAsync(Guid ranchId, Guid lotId, CancellationToken cancellationToken) =>
            throw new NotSupportedException("Planning only looks up animals.");

        public Task<Guid?> FindRanchIdForLotAsync(Guid lotId, CancellationToken cancellationToken) =>
            Task.FromResult<Guid?>(null);

        public Task<IReadOnlyList<CattleSnapshot>> FindActiveByLotAsync(Guid ranchId, Guid lotId, CancellationToken cancellationToken) =>
            throw new NotSupportedException("Planning only looks up animals.");

        public Task<CattleLifecycleSnapshot?> FindLifecycleAsync(Guid cattleId, CancellationToken cancellationToken) =>
            throw new NotSupportedException("Planning only looks up animals.");

        public Task<IReadOnlyList<HerdMemberSnapshot>> GetHerdAsync(Guid ranchId, CancellationToken cancellationToken) =>
            throw new NotSupportedException("Planning only looks up animals.");
    }
}
