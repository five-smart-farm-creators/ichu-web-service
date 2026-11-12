using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Services;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.Planning.Domain;

public class NutritionPlanSchedulerTests
{
    private static readonly Guid RanchId = Guid.Parse("11111111-1111-4111-8111-111111111111");
    private static readonly Guid LotId = Guid.Parse("66666666-6666-4666-8666-666666666666");
    private readonly NutritionPlanScheduler _scheduler = new();

    [Fact]
    public void RationChangeEndsTheOpenPlanTheDayBeforeTheNewOne()
    {
        var current = Plan(new DateOnly(2026, 11, 1));
        var change = Plan(new DateOnly(2026, 11, 15));

        var result = _scheduler.Schedule(change, [current]);

        Assert.True(result.IsSuccess);
        Assert.Equal(new DateOnly(2026, 11, 14), current.ValidTo);
    }

    [Fact]
    public void ChangeOnTheSameDayAsTheOpenPlanIsRefusedAndEndsNothing()
    {
        var current = Plan(new DateOnly(2026, 11, 15));
        var change = Plan(new DateOnly(2026, 11, 15));

        var result = _scheduler.Schedule(change, [current]);

        Assert.Equal(PlanningErrors.NutritionPlanOverlap.Code, result.DomainError.Code);
        Assert.Null(current.ValidTo);
    }

    [Fact]
    public void BoundedPlanInsideAGapIsAccepted()
    {
        var first = Plan(new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 10));
        var second = Plan(new DateOnly(2026, 11, 11), new DateOnly(2026, 11, 20));

        var result = _scheduler.Schedule(second, [first]);

        Assert.True(result.IsSuccess);
        Assert.Equal(new DateOnly(2026, 11, 10), first.ValidTo);
    }

    [Fact]
    public void BoundedPlanSharingADayWithAnotherPlanIsRefused()
    {
        var first = Plan(new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 10));
        var overlapping = Plan(new DateOnly(2026, 11, 10), new DateOnly(2026, 11, 20));

        var result = _scheduler.Schedule(overlapping, [first]);

        Assert.Equal(PlanningErrors.NutritionPlanOverlap.Code, result.DomainError.Code);
    }

    [Fact]
    public void PlanStartingBeforeAFuturePlanIsRefusedWhenItWouldCoverIt()
    {
        var future = Plan(new DateOnly(2026, 12, 1));
        var earlier = Plan(new DateOnly(2026, 11, 1));

        var result = _scheduler.Schedule(earlier, [future]);

        Assert.Equal(PlanningErrors.NutritionPlanOverlap.Code, result.DomainError.Code);
        Assert.Null(future.ValidTo);
    }

    [Fact]
    public void OpenPlanAfterABoundedPlanIsAcceptedWithoutEndingIt()
    {
        var bounded = Plan(new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 10));
        var next = Plan(new DateOnly(2026, 11, 11));

        var result = _scheduler.Schedule(next, [bounded]);

        Assert.True(result.IsSuccess);
        Assert.Equal(new DateOnly(2026, 11, 10), bounded.ValidTo);
    }

    private static NutritionPlan Plan(DateOnly validFrom, DateOnly? validTo = null) =>
        NutritionPlan.Create(new RegisterNutritionPlanCommand(RanchId, LotId, "Ration", validFrom, validTo,
            [new RationItemDraft("Alfalfa hay", RationKind.Forage, 6m)], null, null)).Value!;
}
