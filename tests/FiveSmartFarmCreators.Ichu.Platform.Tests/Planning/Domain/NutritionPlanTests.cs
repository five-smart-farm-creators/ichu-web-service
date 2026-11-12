using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.Planning.Domain;

public class NutritionPlanTests
{
    private static readonly Guid RanchId = Guid.Parse("11111111-1111-4111-8111-111111111111");
    private static readonly Guid LotId = Guid.Parse("66666666-6666-4666-8666-666666666666");
    private static readonly DateOnly Start = new(2026, 11, 1);

    [Fact]
    public void PlanWithItemsIsCreatedWithItsTrimmedNameAndOpenEnd()
    {
        var plan = NutritionPlan.Create(Command(name: "  Dry season ration  ")).Value!;

        Assert.Equal("Dry season ration", plan.Name);
        Assert.Null(plan.ValidTo);
        Assert.True(plan.IsOpenEnded);
        Assert.Single(plan.Items);
    }

    [Fact]
    public void PlanWithoutItemsIsRejected()
    {
        var result = NutritionPlan.Create(Command(items: []));

        Assert.Equal(PlanningErrors.NutritionPlanItemsRequired.Code, result.DomainError.Code);
    }

    [Fact]
    public void EndBeforeStartIsRejected()
    {
        var result = NutritionPlan.Create(Command(validTo: Start.AddDays(-1)));

        Assert.Equal(PlanningErrors.NutritionPlanValidityInvalid.Code, result.DomainError.Code);
    }

    [Fact]
    public void OneDayPlanEndingOnItsStartIsAccepted()
    {
        var result = NutritionPlan.Create(Command(validTo: Start));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void MissingStartIsRejected()
    {
        var result = NutritionPlan.Create(new RegisterNutritionPlanCommand(RanchId, LotId, "Dry season ration", null, null,
            [new RationItemDraft("Alfalfa hay", RationKind.Forage, 6m)], null, null));

        Assert.Equal(PlanningErrors.NutritionPlanValidFromRequired.Code, result.DomainError.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void BlankNameIsRejected(string? name)
    {
        Assert.Equal(PlanningErrors.NutritionPlanNameRequired.Code, NutritionPlan.Create(Command(name: name)).DomainError.Code);
    }

    [Fact]
    public void NameLongerThanEightyCharactersIsRejected()
    {
        var result = NutritionPlan.Create(Command(name: new string('n', NutritionPlan.MaximumNameLength + 1)));

        Assert.Equal(PlanningErrors.NutritionPlanNameTooLong.Code, result.DomainError.Code);
    }

    [Fact]
    public void NotesLongerThanTheLimitAreRejected()
    {
        var result = NutritionPlan.Create(Command(notes: new string('n', NutritionPlan.MaximumNotesLength + 1)));

        Assert.Equal(PlanningErrors.NutritionNotesTooLong.Code, result.DomainError.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.5)]
    [InlineData(3.5)]
    public void TargetDailyGainOutsideItsRangeIsRejected(double gain)
    {
        var result = NutritionPlan.Create(Command(targetDailyGainKilograms: (decimal)gain));

        Assert.Equal(PlanningErrors.TargetDailyGainInvalid.Code, result.DomainError.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(100.5)]
    public void RationQuantityOutsideItsRangeIsRejected(double kilograms)
    {
        var result = NutritionPlan.Create(Command(items: [new RationItemDraft("Alfalfa hay", RationKind.Forage, (decimal)kilograms)]));

        Assert.Equal(PlanningErrors.RationQuantityInvalid.Code, result.DomainError.Code);
    }

    [Fact]
    public void RationItemWithoutFeedIsRejected()
    {
        var result = NutritionPlan.Create(Command(items: [new RationItemDraft("  ", RationKind.Forage, 6m)]));

        Assert.Equal(PlanningErrors.RationFeedRequired.Code, result.DomainError.Code);
    }

    [Fact]
    public void RationItemFeedLongerThanEightyCharactersIsRejected()
    {
        var result = NutritionPlan.Create(Command(items: [new RationItemDraft(new string('f', RationItem.MaximumFeedLength + 1), RationKind.Forage, 6m)]));

        Assert.Equal(PlanningErrors.RationFeedTooLong.Code, result.DomainError.Code);
    }

    [Fact]
    public void RationItemWithoutKnownKindIsRejected()
    {
        var missing = NutritionPlan.Create(Command(items: [new RationItemDraft("Alfalfa hay", null, 6m)]));
        var unknown = NutritionPlan.Create(Command(items: [new RationItemDraft("Alfalfa hay", (RationKind)99, 6m)]));

        Assert.Equal(PlanningErrors.RationKindInvalid.Code, missing.DomainError.Code);
        Assert.Equal(PlanningErrors.RationKindInvalid.Code, unknown.DomainError.Code);
    }

    [Fact]
    public void CloseBeforeEndsAnOpenPlanTheDayBeforeTheChange()
    {
        var plan = NutritionPlan.Create(Command()).Value!;

        var closed = plan.CloseBefore(new DateOnly(2026, 11, 15));

        Assert.True(closed.IsSuccess);
        Assert.Equal(new DateOnly(2026, 11, 14), plan.ValidTo);
        Assert.False(plan.IsOpenEnded);
    }

    [Fact]
    public void CloseBeforeOnTheStartDayIsRefusedAsOverlap()
    {
        var plan = NutritionPlan.Create(Command(validFrom: new DateOnly(2026, 11, 15))).Value!;

        var closed = plan.CloseBefore(new DateOnly(2026, 11, 15));

        Assert.Equal(PlanningErrors.NutritionPlanOverlap.Code, closed.DomainError.Code);
        Assert.Null(plan.ValidTo);
    }

    [Fact]
    public void CloseBeforeRefusesAPlanThatIsAlreadyBounded()
    {
        var plan = NutritionPlan.Create(Command(validTo: new DateOnly(2026, 11, 20))).Value!;

        var closed = plan.CloseBefore(new DateOnly(2026, 11, 15));

        Assert.Equal(PlanningErrors.NutritionPlanOverlap.Code, closed.DomainError.Code);
        Assert.Equal(new DateOnly(2026, 11, 20), plan.ValidTo);
    }

    [Fact]
    public void OverlapsSharesDaysAndTreatsAMissingEndAsOpen()
    {
        var bounded = NutritionPlan.Create(Command(validTo: new DateOnly(2026, 11, 10))).Value!;
        var open = NutritionPlan.Create(Command(validFrom: new DateOnly(2026, 12, 1))).Value!;

        Assert.True(bounded.Overlaps(new DateOnly(2026, 11, 10), new DateOnly(2026, 11, 12)));
        Assert.False(bounded.Overlaps(new DateOnly(2026, 11, 11), null));
        Assert.True(open.Overlaps(new DateOnly(2027, 1, 1), null));
        Assert.False(open.Overlaps(new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30)));
    }

    private static RegisterNutritionPlanCommand Command(
        string? name = "Dry season ration",
        DateOnly? validFrom = null,
        DateOnly? validTo = null,
        IReadOnlyList<RationItemDraft>? items = null,
        decimal? targetDailyGainKilograms = null,
        string? notes = null) =>
        new(RanchId, LotId, name, validFrom ?? Start, validTo,
            items ?? [new RationItemDraft("Alfalfa hay", RationKind.Forage, 6m)], targetDailyGainKilograms, notes);
}
