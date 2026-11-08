using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application.Internal.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.SubscriptionPlans.Application;

public class SubscriptionCommandServiceTests
{
    private const string CurrentTerms = "2026-11";
    private static readonly Guid RanchId = Guid.Parse("11111111-1111-4111-8111-111111111111");
    private static readonly PlanId StandardPlanId = new(Guid.Parse("5b1f0d2e-0c11-4a51-9b01-000000000002"));

    private readonly FakeAccessService _access = new();
    private readonly FakeSubscriptionRepository _subscriptions = new();
    private readonly CountingUnitOfWork _unitOfWork = new();
    private readonly SubscriptionCommandService _service;

    public SubscriptionCommandServiceTests()
    {
        var standard = new Plan(StandardPlanId, PlanTier.Standard, "Standard", new DeviceLimit(200),
            new Money(1800m, "PEN"), new Money(120m, "PEN"), [FeatureCode.Monitoring], isPublished: true);
        _service = new SubscriptionCommandService(_access, new FakePlanRepository(standard), _subscriptions,
            _unitOfWork, Options.Create(new SubscriptionPlansOptions { CurrentTermsVersion = CurrentTerms }),
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
