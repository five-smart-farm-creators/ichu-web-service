using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.Iam.Domain;

public class HerdAdvisoryTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 9, 12, 0, 0, TimeSpan.Zero);
    private static readonly UserAccountId Owner = new(Guid.Parse("22222222-2222-4222-8222-222222222222"));

    [Fact]
    public void Create_WithBlankReason_ReturnsInvalidAdvisory()
    {
        var result = HerdAdvisory.Create(new UserAccountId(Guid.NewGuid()), Guid.NewGuid(), "   ", Now);

        Assert.Equal(IamErrors.InvalidAdvisory, result.DomainError);
    }

    [Fact]
    public void Grant_OfAnAdvisoryThatIsNotPending_ReturnsAdvisoryNotPending()
    {
        var advisory = PendingAdvisory();
        advisory.Grant(Owner, AccessScope.Read, Now.AddDays(1), Now);

        var result = advisory.Grant(Owner, AccessScope.Read, Now.AddDays(1), Now);

        Assert.Equal(IamErrors.AdvisoryNotPending, result.DomainError);
    }

    [Fact]
    public void Grant_WithManageScope_ReturnsInvalidAdvisoryScope()
    {
        var result = PendingAdvisory().Grant(Owner, AccessScope.Manage, Now.AddDays(1), Now);

        Assert.Equal(IamErrors.InvalidAdvisoryScope, result.DomainError);
    }

    [Fact]
    public void Grant_ExpiringAtTheCurrentInstant_ReturnsExpirationNotInFuture()
    {
        var result = PendingAdvisory().Grant(Owner, AccessScope.Read, Now, Now);

        Assert.Equal(IamErrors.ExpirationNotInFuture, result.DomainError);
    }

    [Fact]
    public void Grant_WithReadScope_IsEffectiveUntilItsExpiration()
    {
        var advisory = PendingAdvisory();

        var result = advisory.Grant(Owner, AccessScope.Read, Now.AddDays(1), Now);

        Assert.True(result.IsSuccess);
        Assert.True(advisory.IsValidOn(Now.AddHours(23)));
        Assert.False(advisory.IsValidOn(Now.AddDays(1)));
    }

    [Fact]
    public void End_RejectingAGrantedAdvisory_ReturnsAdvisoryTransitionInvalid()
    {
        var advisory = PendingAdvisory();
        advisory.Grant(Owner, AccessScope.Read, Now.AddDays(1), Now);

        var result = advisory.End(Owner, true, Now);

        Assert.Equal(IamErrors.AdvisoryTransitionInvalid, result.DomainError);
    }

    [Fact]
    public void Expire_AfterTheGrantEnds_MarksTheAdvisoryExpired()
    {
        var advisory = PendingAdvisory();
        advisory.Grant(Owner, AccessScope.Read, Now.AddHours(1), Now);

        advisory.Expire(Now.AddHours(2));

        Assert.Equal(AdvisoryStatus.Expired, advisory.Status);
    }

    private static HerdAdvisory PendingAdvisory() =>
        HerdAdvisory.Create(new UserAccountId(Guid.NewGuid()), Guid.NewGuid(), "Herd health review", Now).Value!;
}
