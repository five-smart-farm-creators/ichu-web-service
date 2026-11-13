using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Internal.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Services;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.Planning.Application;

public class NutritionPlanCommandServiceTests
{
    private static readonly Guid RanchId = Guid.Parse("11111111-1111-4111-8111-111111111111");
    private static readonly Guid OtherRanchId = Guid.Parse("22222222-2222-4222-8222-222222222222");
    private static readonly Guid LotId = Guid.Parse("66666666-6666-4666-8666-666666666666");

    private readonly FakePlanningAccess _access = new() { AccountId = Guid.NewGuid() };
    private readonly FakeLotLookup _lots = new();
    private readonly FakeNutritionPlans _plans = new();
    private readonly NutritionPlanCommandService _service;

    public NutritionPlanCommandServiceTests()
    {
        var guard = new PlanningGuard(_access, new FakeCattleLookup(), new FakeInterventionLookup(), _lots);
        _service = new NutritionPlanCommandService(_plans, new ImmediateTransaction(), new NutritionPlanScheduler(), guard);
    }

    [Fact]
    public async Task Register_ForLotOfAnotherRanch_IsNotFoundAndSavesNothing()
    {
        _lots.Lots.Add((OtherRanchId, LotId));
        _access.DeniedRanches.Add(OtherRanchId);

        var result = await _service.HandleAsync(Command(), CancellationToken.None);

        Assert.Equal(PlanningErrors.LotNotFound.Code, result.DomainError.Code);
        Assert.Empty(_plans.Items);
    }

    [Fact]
    public async Task Register_WithoutManagePermission_IsForbidden()
    {
        _lots.Lots.Add((RanchId, LotId));
        _access.Grants = [PlanningPermission.ReadPlanning];

        var result = await _service.HandleAsync(Command(), CancellationToken.None);

        Assert.Equal(PlanningErrors.RanchAccessDenied.Code, result.DomainError.Code);
        Assert.Empty(_plans.Items);
    }

    [Fact]
    public async Task Register_ChangingTheRation_EndsThePreviousPlanAndSavesTheNewOne()
    {
        _lots.Lots.Add((RanchId, LotId));
        var previous = Stored(new DateOnly(2026, 11, 1));
        _plans.Items.Add(previous);

        var result = await _service.HandleAsync(Command(validFrom: new DateOnly(2026, 11, 15)), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, _plans.Items.Count);
        Assert.Equal(new DateOnly(2026, 11, 14), previous.ValidTo);
    }

    [Fact]
    public async Task Register_OverlappingBoundedPlan_IsRefusedAndTheStoredPlansStayAsTheyWere()
    {
        _lots.Lots.Add((RanchId, LotId));
        var stored = Stored(new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30));
        _plans.Items.Add(stored);

        var result = await _service.HandleAsync(Command(validFrom: new DateOnly(2026, 11, 10), validTo: new DateOnly(2026, 11, 20)),
            CancellationToken.None);

        Assert.Equal(PlanningErrors.NutritionPlanOverlap.Code, result.DomainError.Code);
        Assert.Single(_plans.Items);
        Assert.Equal(new DateOnly(2026, 11, 30), stored.ValidTo);
    }

    private static RegisterNutritionPlanRequest Command(DateOnly? validFrom = null, DateOnly? validTo = null) =>
        new(LotId, "Dry season ration", validFrom ?? new DateOnly(2026, 11, 1), validTo,
            [new RationItemDraft("Alfalfa hay", RationKind.Forage, 6m)], null, null);

    private static NutritionPlan Stored(DateOnly validFrom, DateOnly? validTo = null) =>
        NutritionPlan.Create(new RegisterNutritionPlanCommand(RanchId, LotId, "Dry season ration", validFrom, validTo,
            [new RationItemDraft("Alfalfa hay", RationKind.Forage, 6m)], null, null)).Value!;
}
