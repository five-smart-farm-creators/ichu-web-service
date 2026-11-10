using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application.Internal.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Repositories;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.SubscriptionPlans.Application;

public class SubscriptionCommandServiceTests
{
    private const string CurrentTerms = "2026-11";
    private const long PaidTotalMinor = 1620000; // The 16200.00 PEN total of the request helper, in minor units.
    private static readonly Guid RanchId = Guid.Parse("11111111-1111-4111-8111-111111111111");
    private static readonly PlanId StandardPlanId = new(Guid.Parse("5b1f0d2e-0c11-4a51-9b01-000000000002"));

    private readonly FakeAccessService _access = new();
    private readonly FakeSubscriptionRepository _subscriptions = new();
    private readonly CountingUnitOfWork _unitOfWork = new();
    private readonly FakePaymentGateway _payments = new();
    private readonly SubscriptionCommandService _service;

    public SubscriptionCommandServiceTests()
    {
        var standard = new Plan(StandardPlanId, PlanTier.Standard, "Standard", new DeviceLimit(200),
            new Money(1800m, "PEN"), new Money(120m, "PEN"), [FeatureCode.Monitoring], isPublished: true);
        _service = new SubscriptionCommandService(_access, new FakePlanRepository(standard), _subscriptions,
            _unitOfWork, _payments, Options.Create(new SubscriptionPlansOptions { CurrentTermsVersion = CurrentTerms }),
            new FakeTimeProvider(new DateTimeOffset(2026, 11, 7, 10, 0, 0, TimeSpan.Zero)));
    }

    [Fact]
    public async Task Handle_RequestCreatesPendingSubscriptionAndSavesIt()
    {
        var result = await _service.Handle(Request(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.Created);
        Assert.Equal(SubscriptionStatus.Pending, result.Value.Subscription.Status);
        Assert.Equal(16200m, result.Value.Subscription.Total.Amount);
        Assert.Single(_subscriptions.Items);
        Assert.Equal(1, _unitOfWork.CompletedCount);
    }

    [Fact]
    public async Task Handle_RepeatedOperationReturnsTheStoredSubscriptionWithoutSavingAgain()
    {
        var operationId = Guid.NewGuid();
        var first = await _service.Handle(Request(operationId: operationId), CancellationToken.None);

        var retry = await _service.Handle(Request(operationId: operationId), CancellationToken.None);

        Assert.True(retry.IsSuccess);
        Assert.False(retry.Value!.Created);
        Assert.Equal(first.Value!.Subscription.Id, retry.Value.Subscription.Id);
        Assert.Single(_subscriptions.Items);
        Assert.Equal(1, _unitOfWork.CompletedCount);
    }

    [Fact]
    public async Task Handle_RepeatedOperationWithDifferentDataIsAConflict()
    {
        var operationId = Guid.NewGuid();
        await _service.Handle(Request(operationId: operationId), CancellationToken.None);

        var conflicting = await _service.Handle(Request(headCount: 121, expectedTotal: 16320m, operationId: operationId),
            CancellationToken.None);

        Assert.True(conflicting.IsFailure);
        Assert.Equal(SubscriptionPlansErrors.OperationConflict.Code, conflicting.DomainError.Code);
    }

    [Fact]
    public async Task Handle_RejectsTermsThatAreNoLongerCurrent()
    {
        var result = await _service.Handle(Request(terms: "2026-01"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(SubscriptionPlansErrors.TermsVersionOutdated.Code, result.DomainError.Code);
        Assert.Empty(_subscriptions.Items);
    }

    [Fact]
    public async Task Handle_RejectsExpectedTotalThatNoLongerMatchesTheQuote()
    {
        var result = await _service.Handle(Request(expectedTotal: 15000m), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(SubscriptionPlansErrors.QuoteChanged.Code, result.DomainError.Code);
        Assert.Empty(_subscriptions.Items);
    }

    [Fact]
    public async Task Handle_RejectsNewRequestWhileTheRanchHasAPendingOrActiveSubscription()
    {
        await _service.Handle(Request(), CancellationToken.None);

        var second = await _service.Handle(Request(), CancellationToken.None);

        Assert.True(second.IsFailure);
        Assert.Equal(SubscriptionPlansErrors.SubscriptionAlreadyExists.Code, second.DomainError.Code);
        Assert.Single(_subscriptions.Items);
    }

    [Fact]
    public async Task Handle_RejectsCallerWithoutRanchPermissionAndSavesNothing()
    {
        _access.Outcome = Result<Guid>.Failure(SubscriptionPlansErrors.RanchAccessDenied);

        var result = await _service.Handle(Request(), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(SubscriptionPlansErrors.RanchAccessDenied.Code, result.DomainError.Code);
        Assert.Empty(_subscriptions.Items);
        Assert.Equal(0, _unitOfWork.CompletedCount);
    }

    [Fact]
    public async Task Handle_StartCheckoutCreatesSessionForThePendingSubscriptionAndItsPlan()
    {
        var subscription = await RequestPendingAsync();

        var result = await _service.Handle(new StartSubscriptionCheckoutCommand(RanchId, subscription.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("cs_test_1", result.Value!.SessionId);
        var request = Assert.Single(_payments.CheckoutRequests);
        Assert.Same(subscription, request.Subscription);
        Assert.Equal("Standard", request.PlanName);
        Assert.Equal(SubscriptionStatus.Pending, subscription.Status);
    }

    [Fact]
    public async Task Handle_StartCheckoutRejectsCallerWithoutRanchPermission()
    {
        var subscription = await RequestPendingAsync();
        _access.Outcome = Result<Guid>.Failure(SubscriptionPlansErrors.RanchAccessDenied);

        var result = await _service.Handle(new StartSubscriptionCheckoutCommand(RanchId, subscription.Id), CancellationToken.None);

        Assert.Equal(SubscriptionPlansErrors.RanchAccessDenied.Code, result.DomainError.Code);
        Assert.Empty(_payments.CheckoutRequests);
    }

    [Fact]
    public async Task Handle_StartCheckoutForSubscriptionOfAnotherRanchIsNotFound()
    {
        var subscription = await RequestPendingAsync();

        var result = await _service.Handle(new StartSubscriptionCheckoutCommand(Guid.NewGuid(), subscription.Id), CancellationToken.None);

        Assert.Equal(SubscriptionPlansErrors.SubscriptionNotFound.Code, result.DomainError.Code);
        Assert.Empty(_payments.CheckoutRequests);
    }

    [Fact]
    public async Task Handle_StartCheckoutForActiveSubscriptionIsNotPending()
    {
        var subscription = await RequestPendingAsync();
        await _service.Handle(new ActivateSubscriptionCommand(subscription.Id), CancellationToken.None);

        var result = await _service.Handle(new StartSubscriptionCheckoutCommand(RanchId, subscription.Id), CancellationToken.None);

        Assert.Equal(SubscriptionPlansErrors.SubscriptionNotPending.Code, result.DomainError.Code);
        Assert.Empty(_payments.CheckoutRequests);
    }

    [Fact]
    public async Task Handle_StartCheckoutReturnsTheFailureOfThePaymentProvider()
    {
        var subscription = await RequestPendingAsync();
        _payments.CheckoutResult = Result<CheckoutSession>.Failure(SubscriptionPlansErrors.PaymentProviderUnavailable);

        var result = await _service.Handle(new StartSubscriptionCheckoutCommand(RanchId, subscription.Id), CancellationToken.None);

        Assert.Equal(SubscriptionPlansErrors.PaymentProviderUnavailable.Code, result.DomainError.Code);
        Assert.Equal(SubscriptionStatus.Pending, subscription.Status);
    }

    [Fact]
    public async Task Handle_ConfirmedPaymentActivatesPendingSubscriptionWhenPaidAmountMatchesTheTotal()
    {
        var subscription = await RequestPendingAsync();

        var result = await _service.Handle(new ConfirmSubscriptionPaymentCommand(Confirmation(subscription.Id)), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Active, subscription.Status);
        Assert.Equal(2, _unitOfWork.CompletedCount);
    }

    [Fact]
    public async Task Handle_RepeatedConfirmationOfActiveSubscriptionSucceedsWithoutChange()
    {
        var subscription = await RequestPendingAsync();
        await _service.Handle(new ConfirmSubscriptionPaymentCommand(Confirmation(subscription.Id)), CancellationToken.None);
        var activatedAt = subscription.ActivatedAt;

        var repeated = await _service.Handle(new ConfirmSubscriptionPaymentCommand(Confirmation(subscription.Id)), CancellationToken.None);

        Assert.True(repeated.IsSuccess);
        Assert.Equal(activatedAt, subscription.ActivatedAt);
        Assert.Equal(2, _unitOfWork.CompletedCount);
    }

    [Fact]
    public async Task Handle_UnpaidConfirmationChangesNothing()
    {
        var subscription = await RequestPendingAsync();

        var result = await _service.Handle(new ConfirmSubscriptionPaymentCommand(Confirmation(subscription.Id, paid: false)),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Pending, subscription.Status);
        Assert.Equal(1, _unitOfWork.CompletedCount);
    }

    [Theory]
    [InlineData(1500000, "PEN")] // 15000.00 PEN, below the total
    [InlineData(PaidTotalMinor, "USD")]
    public async Task Handle_ConfirmedPaymentWithDifferentAmountOrCurrencyIsRejected(long amountMinor, string currency)
    {
        var subscription = await RequestPendingAsync();

        var result = await _service.Handle(new ConfirmSubscriptionPaymentCommand(Confirmation(subscription.Id, amountMinor, currency)),
            CancellationToken.None);

        Assert.Equal(SubscriptionPlansErrors.PaymentAmountMismatch.Code, result.DomainError.Code);
        Assert.Equal(SubscriptionStatus.Pending, subscription.Status);
    }

    [Fact]
    public async Task Handle_ConfirmedPaymentOfUnknownSubscriptionIsNotFound()
    {
        var result = await _service.Handle(new ConfirmSubscriptionPaymentCommand(Confirmation(new SubscriptionId(Guid.NewGuid()))),
            CancellationToken.None);

        Assert.Equal(SubscriptionPlansErrors.SubscriptionNotFound.Code, result.DomainError.Code);
    }

    [Fact]
    public async Task Handle_ReceivedEventWithVerifiedSignatureAppliesItsPayment()
    {
        var subscription = await RequestPendingAsync();
        _payments.ConfirmationResult = Result<PaymentConfirmation>.Success(Confirmation(subscription.Id));

        var result = await _service.Handle(new ReceivePaymentEventCommand("{}", "t=1,v1=signature"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Active, subscription.Status);
    }

    [Fact]
    public async Task Handle_ReceivedEventWithInvalidSignatureChangesNothing()
    {
        var subscription = await RequestPendingAsync();
        _payments.ConfirmationResult = Result<PaymentConfirmation>.Failure(SubscriptionPlansErrors.PaymentSignatureInvalid);

        var result = await _service.Handle(new ReceivePaymentEventCommand("{}", "forged"), CancellationToken.None);

        Assert.Equal(SubscriptionPlansErrors.PaymentSignatureInvalid.Code, result.DomainError.Code);
        Assert.Equal(SubscriptionStatus.Pending, subscription.Status);
    }

    private async Task<Subscription> RequestPendingAsync()
    {
        var result = await _service.Handle(Request(), CancellationToken.None);
        return result.Value!.Subscription;
    }

    private static PaymentConfirmation Confirmation(SubscriptionId subscriptionId, long amountMinor = PaidTotalMinor, string currency = "PEN",
        bool paid = true) =>
        new("evt_test_1", "cs_test_1", subscriptionId, amountMinor, currency, paid);

    private static RequestSubscriptionCommand Request(int headCount = 120, decimal expectedTotal = 16200m,
        string terms = CurrentTerms, Guid? operationId = null) =>
        new(RanchId, StandardPlanId, headCount, expectedTotal, terms, operationId ?? Guid.NewGuid(), "Dashboard");

    private sealed class FakeAccessService : ISubscriptionAccessService
    {
        public Result<Guid> Outcome { get; set; } = Result<Guid>.Success(Guid.NewGuid());

        public Task<Result<Guid>> EnsureManageAccessAsync(Guid ranchId, CancellationToken cancellationToken) =>
            Task.FromResult(Outcome);

        public Task<Result<Guid>> EnsureReadAccessAsync(Guid ranchId, CancellationToken cancellationToken) =>
            Task.FromResult(Outcome);
    }

    private sealed class FakeSubscriptionRepository : ISubscriptionRepository
    {
        public List<Subscription> Items { get; } = [];

        public Task AddAsync(Subscription subscription, CancellationToken cancellationToken = default)
        {
            Items.Add(subscription);
            return Task.CompletedTask;
        }

        public Task<Subscription?> FindByIdAsync(SubscriptionId id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Items.SingleOrDefault(subscription => subscription.Id == id));

        public Task<Subscription?> FindByOperationIdAsync(Guid operationId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Items.SingleOrDefault(subscription => subscription.OperationId == operationId));

        public Task<Subscription?> FindCurrentByRanchAsync(Guid ranchId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Items
                .Where(subscription => subscription.RanchId == ranchId
                    && (subscription.Status == SubscriptionStatus.Pending || subscription.Status == SubscriptionStatus.Active))
                .OrderByDescending(subscription => subscription.RequestedAt)
                .FirstOrDefault());
    }

    private sealed class FakePlanRepository(params Plan[] plans) : IPlanRepository
    {
        public Task<Plan?> FindByIdAsync(PlanId id, CancellationToken cancellationToken = default) =>
            Task.FromResult(plans.SingleOrDefault(plan => plan.Id == id));

        public Task<IReadOnlyList<Plan>> ListPublishedAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Plan>>(plans.Where(plan => plan.IsPublished).ToList());
    }

    private sealed class FakePaymentGateway : IPaymentGateway
    {
        public Result<CheckoutSession> CheckoutResult { get; set; } = Result<CheckoutSession>.Success(new CheckoutSession(
            "cs_test_1", new Uri("https://checkout.example.test/cs_test_1"), new DateTimeOffset(2026, 11, 7, 10, 30, 0, TimeSpan.Zero)));

        public Result<PaymentConfirmation> ConfirmationResult { get; set; } =
            Result<PaymentConfirmation>.Failure(SubscriptionPlansErrors.PaymentEventIgnored);

        public List<(Subscription Subscription, string PlanName)> CheckoutRequests { get; } = [];

        public Task<Result<CheckoutSession>> CreateCheckoutSessionAsync(Subscription subscription, string planName, CancellationToken cancellationToken)
        {
            CheckoutRequests.Add((subscription, planName));
            return Task.FromResult(CheckoutResult);
        }

        public Result<PaymentConfirmation> ReadConfirmation(string payload, string signatureHeader) => ConfirmationResult;
    }

    private sealed class CountingUnitOfWork : IUnitOfWork
    {
        public int CompletedCount { get; private set; }

        public Task CompleteAsync(CancellationToken cancellationToken = default)
        {
            CompletedCount++;
            return Task.CompletedTask;
        }
    }
}
