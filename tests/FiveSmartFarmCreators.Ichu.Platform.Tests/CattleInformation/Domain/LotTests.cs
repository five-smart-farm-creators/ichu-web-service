using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.CattleInformation.Domain;

public class LotTests
{
    private static readonly Guid RanchId = Guid.Parse("11111111-1111-4111-8111-111111111111");
    private static readonly Guid OtherRanchId = Guid.Parse("22222222-2222-4222-8222-222222222222");
    private static readonly DateOnly RegisteredOn = new(2026, 5, 1);
    private static readonly DateTimeOffset AssignedAt = new(2026, 11, 7, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_RejectsABlankName()
    {
        var result = Lot.Create(new CreateLotCommand(RanchId, "   ", null));

        Assert.Equal(CattleInformationErrors.InvalidLotName.Code, result.DomainError.Code);
    }

    [Fact]
    public void AddCattle_OfAnActiveAnimalAddsOneMembership()
    {
        var lot = NewLot();
        var cattle = ActiveCattle(RanchId);

        var result = lot.AddCattle(cattle, lot.Version, AssignedAt);

        Assert.True(result.IsSuccess);
        Assert.Equal(cattle.Id, Assert.Single(lot.Memberships).CattleId);
    }

    [Fact]
    public void AddCattle_OfAnAnimalAlreadyInTheLotChangesNothing()
    {
        var lot = NewLot();
        var cattle = ActiveCattle(RanchId);
        lot.AddCattle(cattle, lot.Version, AssignedAt);
        var version = lot.Version;

        var result = lot.AddCattle(cattle, version, AssignedAt);

        Assert.True(result.IsSuccess);
        Assert.Single(lot.Memberships);
        Assert.Equal(version, lot.Version);
    }

    [Fact]
    public void AddCattle_RejectsAnInactiveAnimal()
    {
        var lot = NewLot();
        var cattle = ActiveCattle(RanchId);
        cattle.Deactivate(new DeactivateCattleCommand(RanchId, cattle.Id, ExitReason.Sale, RegisteredOn.AddMonths(1), cattle.Version),
            RegisteredOn.AddMonths(1), AssignedAt);

        var result = lot.AddCattle(cattle, lot.Version, AssignedAt);

        Assert.Equal(CattleInformationErrors.InactiveCattleAssignment.Code, result.DomainError.Code);
        Assert.Empty(lot.Memberships);
    }

    [Fact]
    public void AddCattle_WithAStaleLotVersionIsAVersionMismatch()
    {
        var lot = NewLot();

        var result = lot.AddCattle(ActiveCattle(RanchId), Guid.NewGuid(), AssignedAt);

        Assert.Equal(CattleInformationErrors.LotVersionMismatch.Code, result.DomainError.Code);
        Assert.Empty(lot.Memberships);
    }

    [Fact]
    public void AddCattle_RejectsAnAnimalOfAnotherRanch()
    {
        var lot = NewLot();

        var result = lot.AddCattle(ActiveCattle(OtherRanchId), lot.Version, AssignedAt);

        Assert.Equal(CattleInformationErrors.CattleFromAnotherRanch.Code, result.DomainError.Code);
    }

    private static Lot NewLot() => Lot.Create(new CreateLotCommand(RanchId, "North pasture", null)).Value!;

    private static Cattle ActiveCattle(Guid ranchId) =>
        Cattle.Register(new RegisterCattleCommand(ranchId, "A-1", Breed.Criollo, Sex.Female, new DateOnly(2026, 1, 1), LifeStage.Heifer),
            RegisteredOn).Value!;
}
