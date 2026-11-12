using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Internal.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Errors;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.Planning.Application;

public class WithdrawalPeriodCommandServiceTests
{
    private static readonly Guid RanchId = Guid.Parse("11111111-1111-4111-8111-111111111111");
    private static readonly Guid OtherRanchId = Guid.Parse("22222222-2222-4222-8222-222222222222");
    private static readonly Guid CattleId = Guid.Parse("33333333-3333-4333-8333-333333333333");
    private static readonly DateTimeOffset StartsAt = new(2026, 11, 10, 0, 0, 0, TimeSpan.Zero);

    private readonly FakePlanningAccess _access = new() { AccountId = Guid.NewGuid() };
    private readonly FakeCattleLookup _cattle = new();
    private readonly FakeWithdrawalPeriods _periods = new();
    private readonly CountingUnitOfWork _unitOfWork = new();
    private readonly WithdrawalPeriodCommandService _service;

    public WithdrawalPeriodCommandServiceTests()
    {
        var guard = new PlanningGuard(_access, _cattle, new FakeInterventionLookup(), new FakeLotLookup());
        _service = new WithdrawalPeriodCommandService(_periods, _unitOfWork, guard);
    }

    [Fact]
    public async Task Record_WithAnAnimalOfAnotherRanch_IsNotFoundAndSavesNothing()
    {
        _cattle.Members.Add((OtherRanchId, CattleId));

        var result = await _service.HandleAsync(
            new RecordWithdrawalPeriodCommand(RanchId, CattleId, null, StartsAt, StartsAt.AddDays(7)), CancellationToken.None);

        Assert.Equal(PlanningErrors.CattleNotFound.Code, result.DomainError.Code);
        Assert.Empty(_periods.Items);
        Assert.Equal(0, _unitOfWork.CompletedCount);
    }

    [Fact]
    public async Task Record_WithoutClinicalPermission_IsForbidden()
    {
        _cattle.Members.Add((RanchId, CattleId));
        _access.Grants = [PlanningPermission.ReadPlanning];

        var result = await _service.HandleAsync(
            new RecordWithdrawalPeriodCommand(RanchId, CattleId, null, StartsAt, StartsAt.AddDays(7)), CancellationToken.None);

        Assert.Equal(PlanningErrors.RanchAccessDenied.Code, result.DomainError.Code);
        Assert.Empty(_periods.Items);
    }

    [Fact]
    public async Task Record_ForAnAnimalOfTheRanch_IsSaved()
    {
        _cattle.Members.Add((RanchId, CattleId));

        var result = await _service.HandleAsync(
            new RecordWithdrawalPeriodCommand(RanchId, CattleId, null, StartsAt, StartsAt.AddDays(7)), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(_periods.Items);
        Assert.Equal(1, _unitOfWork.CompletedCount);
    }
}
