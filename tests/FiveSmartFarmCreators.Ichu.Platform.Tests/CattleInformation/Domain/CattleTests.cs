using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Events;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.CattleInformation.Domain;

public class CattleTests
{
    private static readonly Guid RanchId = Guid.Parse("11111111-1111-4111-8111-111111111111");
    private static readonly DateOnly BirthDate = new(2026, 1, 1);
    private static readonly DateOnly RegisteredOn = new(2026, 5, 1);
    private static readonly DateOnly Today = new(2026, 11, 7);
    private static readonly DateTimeOffset RecordedAt = new(2026, 11, 7, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Register_CreatesAnActiveAnimalWhoseFirstStageStartsOnTheRegistrationDate()
    {
        var cattle = RegisteredCattle();

        Assert.Equal(CattleStatus.Active, cattle.Status);
        Assert.Equal(LifeStage.Calf, cattle.CurrentStage);
        var period = Assert.Single(cattle.StagePeriods);
        Assert.Equal(RegisteredOn, period.ValidFrom);
        Assert.Null(period.ValidTo);
        Assert.IsType<CattleRegisteredEvent>(Assert.Single(cattle.DomainEvents));
    }

    [Fact]
    public void Register_ReturnsABusinessErrorForAnEarTagThatIsBlank()
    {
        var result = Cattle.Register(CalfCommand(earTag: "   "), RegisteredOn);

        Assert.True(result.IsFailure);
        Assert.Equal(CattleInformationErrors.InvalidEarTag.Code, result.DomainError.Code);
    }

    [Fact]
    public void Register_RejectsABirthDateInTheFuture()
    {
        var result = Cattle.Register(CalfCommand() with { BirthDate = Today.AddDays(1) }, Today);

        Assert.Equal(CattleInformationErrors.InvalidBirthDate.Code, result.DomainError.Code);
    }

    [Fact]
    public void Update_WithAStaleVersionIsAVersionMismatchAndChangesNothing()
    {
        var cattle = RegisteredCattle();

        var result = cattle.Update(
            new UpdateCattleCommand(RanchId, cattle.Id, "A-2", Breed.Fleckvieh, Sex.Female, BirthDate, Guid.NewGuid()),
            Today);

        Assert.Equal(CattleInformationErrors.CattleVersionMismatch.Code, result.DomainError.Code);
        Assert.Equal("A-1", cattle.EarTag.Value);
    }

    [Fact]
    public void Update_WithTheCurrentVersionCorrectsTheIdentity()
    {
        var cattle = RegisteredCattle();
        var previousVersion = cattle.Version;

        var result = cattle.Update(
            new UpdateCattleCommand(RanchId, cattle.Id, "A-2", Breed.Fleckvieh, Sex.Male, BirthDate, previousVersion),
            Today);

        Assert.True(result.IsSuccess);
        Assert.Equal("A-2", cattle.EarTag.Value);
        Assert.Equal(Sex.Male, cattle.Sex);
        Assert.NotEqual(previousVersion, cattle.Version);
    }

    [Fact]
    public void ChangeStage_ClosesThePreviousPeriodAndOpensTheNewOneOnTheEffectiveDate()
    {
        var cattle = RegisteredCattle();
        var effectiveDate = new DateOnly(2026, 10, 1);

        var result = cattle.ChangeStage(
            new ChangeLifeStageCommand(RanchId, cattle.Id, LifeStage.Heifer, effectiveDate, cattle.Version), Today);

        Assert.True(result.IsSuccess);
        Assert.Equal(LifeStage.Heifer, cattle.CurrentStage);
        Assert.Equal(2, cattle.StagePeriods.Count);
        Assert.Equal(effectiveDate, cattle.StagePeriods.Single(period => period.ValidTo is null).ValidFrom);
        Assert.Equal(effectiveDate, cattle.StagePeriods.Single(period => period.LifeStage == LifeStage.Calf).ValidTo);
        Assert.IsType<CattleLifeStageChangedEvent>(cattle.DomainEvents.Last());
    }

    [Fact]
    public void ChangeStage_RejectsAnEffectiveDateBeforeTheCurrentPeriod()
    {
        var cattle = RegisteredCattle();

        var result = cattle.ChangeStage(
            new ChangeLifeStageCommand(RanchId, cattle.Id, LifeStage.Heifer, RegisteredOn.AddDays(-1), cattle.Version), Today);

        Assert.Equal(CattleInformationErrors.InvalidLifeStageDate.Code, result.DomainError.Code);
        Assert.Equal(LifeStage.Calf, cattle.CurrentStage);
    }

    [Fact]
    public void ChangeStage_ToTheStageAlreadyCurrentIsAConflict()
    {
        var cattle = RegisteredCattle();

        var result = cattle.ChangeStage(
            new ChangeLifeStageCommand(RanchId, cattle.Id, LifeStage.Calf, Today, cattle.Version), Today);

        Assert.Equal(CattleInformationErrors.LifeStageAlreadyCurrent.Code, result.DomainError.Code);
    }

    [Fact]
    public void Deactivate_ClosesTheStageAndRaisesTheExitEventWithTheRecordingInstant()
    {
        var cattle = RegisteredCattle();

        var result = cattle.Deactivate(
            new DeactivateCattleCommand(RanchId, cattle.Id, ExitReason.Death, Today, cattle.Version), Today, RecordedAt);

        Assert.True(result.IsSuccess);
        Assert.Equal(CattleStatus.Inactive, cattle.Status);
        Assert.Equal(ExitReason.Death, cattle.ExitReason);
        Assert.Equal(Today, cattle.StagePeriods.Single().ValidTo);
        var exit = Assert.IsType<CattleDeactivatedEvent>(cattle.DomainEvents.Last());
        Assert.Equal(RecordedAt, exit.DeactivatedAt);
        Assert.Equal(cattle.Id, exit.CattleId);
    }

    [Fact]
    public void Deactivate_AnAlreadyInactiveAnimalCannotChangeAgain()
    {
        var cattle = RegisteredCattle();
        cattle.Deactivate(new DeactivateCattleCommand(RanchId, cattle.Id, ExitReason.Sale, Today, cattle.Version), Today, RecordedAt);

        var result = cattle.ChangeStage(
            new ChangeLifeStageCommand(RanchId, cattle.Id, LifeStage.Heifer, Today, cattle.Version), Today);

        Assert.Equal(CattleInformationErrors.CattleInactive.Code, result.DomainError.Code);
    }

    private static RegisterCattleCommand CalfCommand(string earTag = "A-1") =>
        new(RanchId, earTag, Breed.BrownSwiss, Sex.Female, BirthDate, LifeStage.Calf);

    private static Cattle RegisteredCattle() => Cattle.Register(CalfCommand(), RegisteredOn).Value!;
}
