using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Events;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.SubscriptionPlans.Domain;

public class SubscriptionTests
{
    private static readonly Guid RanchId = Guid.Parse("11111111-1111-4111-8111-111111111111");
    private static readonly DateTimeOffset RequestedAt = new(2026, 11, 7, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Request_StartsPendingWithTheQuotedAmountsAndRaisesEvent()
    {
        var subscription = RequestPending();

        Assert.Equal(SubscriptionStatus.Pending, subscription.Status);
        Assert.Equal(1800m, subscription.AnnualFee.Amount);
        Assert.Equal(14400m, subscription.DevicesCost.Amount);
        Assert.Equal(16200m, subscription.Total.Amount);
        Assert.Equal(RequestedAt, subscription.RequestedAt);
        Assert.IsType<SubscriptionRequestedEvent>(Assert.Single(subscription.DomainEvents));
    }

    [Fact]
    public void DeviceUnitPrice_IsTheQuotedPricePerCollar()
    {
        var subscription = RequestPending();

        Assert.Equal(new Money(120m, "PEN"), subscription.DeviceUnitPrice);
    }

    [Fact]
    public void Request_TrimsAcquisitionSourceAndDropsBlankValues()
    {
        Assert.Equal("Dashboard", RequestPending(acquisitionSource: "  Dashboard ").AcquisitionSource);
        Assert.Null(RequestPending(acquisitionSource: "   ").AcquisitionSource);
    }

    [Fact]
    public void Activate_FromPendingGrantsOneYearOfCoverage()
    {
        var subscription = RequestPending();
        var activatedAt = RequestedAt.AddDays(3);

        var result = subscription.Activate(activatedAt);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Active, subscription.Status);
        Assert.Equal(activatedAt.AddYears(1), subscription.ValidUntil);
        Assert.Contains(subscription.DomainEvents, domainEvent => domainEvent is SubscriptionActivatedEvent);
    }

    [Fact]
    public void Activate_RejectsSubscriptionThatIsNotPending()
    {
        var subscription = RequestPending();
        subscription.Activate(RequestedAt);

        var result = subscription.Activate(RequestedAt.AddDays(1));

        Assert.True(result.IsFailure);
        Assert.Equal(SubscriptionPlansErrors.InvalidStatusTransition.Code, result.DomainError.Code);
    }

    [Fact]
    public void Cancel_RecordsTheTrimmedReasonAndRaisesEvent()
    {
        var subscription = RequestPending();
        subscription.Activate(RequestedAt);

        var result = subscription.Cancel("  Herd sold  ", RequestedAt.AddMonths(2));

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Cancelled, subscription.Status);
        Assert.Equal("Herd sold", subscription.CancellationReason);
        Assert.Equal(RequestedAt.AddMonths(2), subscription.CancelledAt);
        Assert.Contains(subscription.DomainEvents, domainEvent => domainEvent is SubscriptionCancelledEvent);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Cancel_RequiresANonBlankReason(string reason)
    {
        var result = RequestPending().Cancel(reason, RequestedAt);

        Assert.True(result.IsFailure);
        Assert.Equal(SubscriptionPlansErrors.CancellationReasonRequired.Code, result.DomainError.Code);
    }

    [Fact]
    public void Cancel_RejectsReasonLongerThanTheLimit()
    {
        var reason = new string('x', Subscription.MaximumCancellationReasonLength + 1);

        var result = RequestPending().Cancel(reason, RequestedAt);

        Assert.True(result.IsFailure);
        Assert.Equal(SubscriptionPlansErrors.CancellationReasonRequired.Code, result.DomainError.Code);
    }

    [Fact]
    public void Cancel_RejectsSubscriptionThatIsAlreadyCancelled()
    {
        var subscription = RequestPending();
        subscription.Cancel("Duplicate request", RequestedAt);

        var result = subscription.Cancel("Duplicate request", RequestedAt.AddHours(1));

        Assert.True(result.IsFailure);
        Assert.Equal(SubscriptionPlansErrors.InvalidStatusTransition.Code, result.DomainError.Code);
    }

    [Fact]
    public void IsActiveAt_IsFalseBeforeActivationAndOnceValidityEnds()
    {
        var subscription = RequestPending();
        Assert.False(subscription.IsActiveAt(RequestedAt));

        subscription.Activate(RequestedAt);

        Assert.True(subscription.IsActiveAt(RequestedAt.AddYears(1).AddSeconds(-1)));
        Assert.False(subscription.IsActiveAt(RequestedAt.AddYears(1)));
    }

    [Fact]
    public void Matches_IdentifiesARepeatedRequestByRanchPlanHeadCountAndTerms()
    {
        var operationId = Guid.NewGuid();
        var quote = StandardQuote();
        var subscription = Subscription.Request(RanchId, quote, "2026-11", operationId, null, RequestedAt);
        var repeated = new RequestSubscriptionCommand(RanchId, quote.PlanId, 120, 16200m, "2026-11", operationId, null);

        Assert.True(subscription.Matches(repeated));
        Assert.False(subscription.Matches(repeated with { TermsVersion = "2026-01" }));
        Assert.False(subscription.Matches(repeated with { HeadCount = 121 }));
    }

    private static Subscription RequestPending(string? acquisitionSource = null) =>
        Subscription.Request(RanchId, StandardQuote(), "2026-11", Guid.NewGuid(), acquisitionSource, RequestedAt);

    private static PlanQuote StandardQuote() =>
        new Plan(new PlanId(Guid.Parse("5b1f0d2e-0c11-4a51-9b01-000000000002")), PlanTier.Standard, "Standard",
            new DeviceLimit(200), new Money(1800m, "PEN"), new Money(120m, "PEN"),
            [FeatureCode.Monitoring, FeatureCode.Alerts], isPublished: true)
            .EstimateFor(new HeadCount(120))
            .Value!;
}
