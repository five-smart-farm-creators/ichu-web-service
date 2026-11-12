using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Internal.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.ValueObjects;
using Microsoft.Extensions.Time.Testing;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.Planning.Application;

public class NutritionPlanQueryServiceTests
{
    private static readonly Guid RanchId = Guid.Parse("11111111-1111-4111-8111-111111111111");
    private static readonly Guid LotId = Guid.Parse("66666666-6666-4666-8666-666666666666");

    private readonly FakePlanningAccess _access = new() { AccountId = Guid.NewGuid() };
    private readonly FakeLotLookup _lots = new();
    private readonly FakeNutritionPlans _plans = new();

    [Fact]
    public async Task CurrentPlan_WithoutDate_UsesTheUtcDateOfTheClock()
    {
        _lots.Lots.Add((RanchId, LotId));
        _plans.Items.Add(Stored(new DateOnly(2026, 11, 1)));
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 11, 10, 23, 0, 0, TimeSpan.Zero));

        var result = await Service(clock).HandleAsync(new GetCurrentNutritionPlanQuery(RanchId, LotId, null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Dry season ration", result.Value!.Name);
    }

    [Fact]
    public async Task CurrentPlan_BeforeTheFirstPlan_IsNotFound()
    {
        _lots.Lots.Add((RanchId, LotId));
        _plans.Items.Add(Stored(new DateOnly(2026, 11, 1)));

        var result = await Service().HandleAsync(new GetCurrentNutritionPlanQuery(RanchId, LotId, new DateOnly(2026, 10, 31)),
            CancellationToken.None);

        Assert.Equal(PlanningErrors.NoCurrentNutritionPlan.Code, result.DomainError.Code);
    }

    [Fact]
    public async Task PlansOfTheLot_ListNewestFirst()
    {
        _lots.Lots.Add((RanchId, LotId));
        _plans.Items.Add(Stored(new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 14)));
        _plans.Items.Add(Stored(new DateOnly(2026, 11, 15)));

        var result = await Service().HandleAsync(new GetNutritionPlansByLotQuery(RanchId, LotId), CancellationToken.None);

        Assert.Equal(new[] { new DateOnly(2026, 11, 15), new DateOnly(2026, 11, 1) }, result.Value!.Select(plan => plan.ValidFrom));
    }

    [Fact]
    public async Task PlansOfAnUnknownLot_AreNotFound()
    {
        var result = await Service().HandleAsync(new GetNutritionPlansByLotQuery(RanchId, LotId), CancellationToken.None);

        Assert.Equal(PlanningErrors.LotNotFound.Code, result.DomainError.Code);
    }

    private NutritionPlanQueryService Service(TimeProvider? clock = null)
    {
        var guard = new PlanningGuard(_access, new FakeCattleLookup(), new FakeInterventionLookup(), _lots);
        return new NutritionPlanQueryService(_plans, guard, clock ?? TimeProvider.System);
    }

    private static NutritionPlan Stored(DateOnly validFrom, DateOnly? validTo = null) =>
        NutritionPlan.Create(new RegisterNutritionPlanCommand(RanchId, LotId, "Dry season ration", validFrom, validTo,
            [new RationItemDraft("Alfalfa hay", RationKind.Forage, 6m)], null, null)).Value!;
}
