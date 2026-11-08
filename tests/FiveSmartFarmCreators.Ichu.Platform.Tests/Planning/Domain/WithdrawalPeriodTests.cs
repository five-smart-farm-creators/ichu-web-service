using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Errors;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.Planning.Domain;

public class WithdrawalPeriodTests
{
    private static readonly Guid RanchId = Guid.Parse("11111111-1111-4111-8111-111111111111");
    private static readonly Guid CattleId = Guid.Parse("33333333-3333-4333-8333-333333333333");
    private static readonly DateTimeOffset StartsAt = new(2026, 11, 10, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_EndingBeforeItStarts_IsRejected()
    {
        var result = WithdrawalPeriod.Create(RanchId, CattleId, null, StartsAt, StartsAt.AddDays(-1));

        Assert.Equal(PlanningErrors.WithdrawalDateRangeInvalid.Code, result.DomainError.Code);
    }

    [Fact]
    public void Create_KeepsTheExplicitBoundsWithoutCalculatingThem()
    {
        var endsAt = StartsAt.AddDays(14);

        var result = WithdrawalPeriod.Create(RanchId, CattleId, null, StartsAt, endsAt);

        Assert.True(result.IsSuccess);
        Assert.Equal(StartsAt, result.Value!.StartsAt);
        Assert.Equal(endsAt, result.Value.EndsAt);
    }
}
