using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.Acl;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.CattleInformation.Application;

public class CattleInformationContextFacadeTests
{
    private static readonly Guid RanchId = Guid.Parse("11111111-1111-4111-8111-111111111111");
    private static readonly DateOnly BirthDate = new(2026, 1, 1);
    private static readonly DateOnly RegisteredOn = new(2026, 5, 1);
    private static readonly DateOnly Today = new(2026, 11, 7);
    private static readonly DateTimeOffset AssignedAt = new(2026, 11, 1, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task GetHerd_ListsActiveAndExitedAnimalsWithTheirCurrentLot()
    {
        var cattle = new FakeCattleRepository();
        var lots = new FakeLotRepository();
        var active = Register(cattle, "A-1");
        var exited = Register(cattle, "A-2");
        Assert.True(exited.Deactivate(new DeactivateCattleCommand(RanchId, exited.Id, ExitReason.Sale, Today, exited.Version), Today, AssignedAt).IsSuccess);
        var lot = Lot.Create(new CreateLotCommand(RanchId, "Heifers", null)).Value!;
        Assert.True(lot.AddCattle(active, lot.Version, AssignedAt).IsSuccess);
        lots.Items.Add(lot);

        var herd = await Facade(cattle, lots).GetHerdAsync(RanchId, CancellationToken.None);

        Assert.Equal(2, herd.Count);
        var activeMember = herd.Single(member => member.CattleId == active.Id.Value);
        Assert.Equal(lot.Id.Value, activeMember.LotId);
        Assert.Null(activeMember.ExitReason);
        var exitedMember = herd.Single(member => member.CattleId == exited.Id.Value);
        Assert.Null(exitedMember.LotId);
        Assert.Equal("Sale", exitedMember.ExitReason);
        Assert.Equal(Today, exitedMember.ExitDate);
    }

    [Fact]
    public async Task GetHerd_UsesTheLatestAssignmentWhenAnAnimalHoldsSeveralLots()
    {
        var cattle = new FakeCattleRepository();
        var lots = new FakeLotRepository();
        var animal = Register(cattle, "A-1");
        var calves = Lot.Create(new CreateLotCommand(RanchId, "Calves", null)).Value!;
        var heifers = Lot.Create(new CreateLotCommand(RanchId, "Heifers", null)).Value!;
        Assert.True(calves.AddCattle(animal, calves.Version, AssignedAt).IsSuccess);
        Assert.True(heifers.AddCattle(animal, heifers.Version, AssignedAt.AddDays(10)).IsSuccess);
        lots.Items.Add(calves);
        lots.Items.Add(heifers);

        var herd = await Facade(cattle, lots).GetHerdAsync(RanchId, CancellationToken.None);

        Assert.Equal(heifers.Id.Value, Assert.Single(herd).LotId);
    }

    [Fact]
    public async Task GetHerd_ReturnsNothingWhenTheCallerCannotReadTheRanch()
    {
        var cattle = new FakeCattleRepository();
        Register(cattle, "A-1");
        var facade = new CattleInformationContextFacade(cattle, new FakeLotRepository(), new FakeAccess(granted: false));

        var herd = await facade.GetHerdAsync(RanchId, CancellationToken.None);

        Assert.Empty(herd);
    }

    private static CattleInformationContextFacade Facade(FakeCattleRepository cattle, FakeLotRepository lots) =>
        new(cattle, lots, new FakeAccess(granted: true));

    private static Cattle Register(FakeCattleRepository cattle, string earTag)
    {
        var animal = Cattle.Register(new RegisterCattleCommand(RanchId, earTag, Breed.BrownSwiss, Sex.Female, BirthDate, LifeStage.Calf),
            RegisteredOn).Value!;
        cattle.Items.Add(animal);
        return animal;
    }

    private sealed class FakeAccess(bool granted) : ICattleAccessService
    {
        public Task<Result> EnsureReadAccessAsync(Guid ranchId, CancellationToken cancellationToken) => Task.FromResult(Outcome());

        public Task<Result> EnsureManageAccessAsync(Guid ranchId, CancellationToken cancellationToken) => Task.FromResult(Outcome());

        private Result Outcome() => granted ? Result.Success() : Result.Failure(CattleInformationErrors.RanchAccessDenied);
    }

    private sealed class FakeCattleRepository : ICattleRepository
    {
        public List<Cattle> Items { get; } = [];

        public Task<IReadOnlyList<Cattle>> FindByRanchAsNoTrackingAsync(Guid ranchId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Cattle>>(Items.Where(cattle => cattle.RanchId == ranchId).ToList());

        public Task<Cattle?> FindByIdAsync(Guid ranchId, CattleId cattleId, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("The herd read does not look up single animals.");

        public Task<Cattle?> FindByIdAsNoTrackingAsync(CattleId cattleId, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("The herd read does not look up single animals.");

        public Task<CattleId?> FindIdByEarTagAsync(Guid ranchId, EarTag earTag, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("The herd read does not search by ear tag.");

        public Task<IReadOnlyList<Cattle>> SearchAsync(SearchCattleQuery query, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("The herd read does not page the search.");

        public Task AddAsync(Cattle cattle, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("The herd read does not add animals.");
    }

    private sealed class FakeLotRepository : ILotRepository
    {
        public List<Lot> Items { get; } = [];

        public Task<IReadOnlyList<LotMembership>> FindMembershipsByRanchAsNoTrackingAsync(Guid ranchId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<LotMembership>>(Items.SelectMany(lot => lot.Memberships)
                .Where(membership => membership.RanchId == ranchId)
                .ToList());

        public Task<Lot?> FindByIdAsync(Guid ranchId, LotId lotId, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("The herd read uses memberships only.");

        public Task<IReadOnlyList<Lot>> FindByRanchAsync(Guid ranchId, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("The herd read uses memberships only.");

        public Task AddAsync(Lot lot, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("The herd read does not add lots.");
    }
}
