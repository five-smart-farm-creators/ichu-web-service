using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.DashboardAnalytics.Application;

internal sealed class FakeAnalyticsAccess : IAnalyticsAccessControl
{
    public Guid? AccountId { get; init; } = Guid.Parse("44444444-4444-4444-8444-444444444444");

    public bool CanRead { get; init; } = true;

    public IReadOnlyList<Guid> AccessibleRanches { get; init; } = [];

    public Task<Guid?> GetCurrentAccountIdAsync(CancellationToken cancellationToken) => Task.FromResult(AccountId);

    public Task<bool> CanReadRanchAsync(Guid accountId, Guid ranchId, CancellationToken cancellationToken) => Task.FromResult(CanRead);

    public Task<IReadOnlyList<Guid>> GetAccessibleRanchIdsAsync(Guid accountId, CancellationToken cancellationToken) =>
        Task.FromResult(AccessibleRanches);
}

internal sealed class FakeAnalyticsSource : IAnalyticsSource
{
    public AnalyticsSnapshot? Snapshot { get; init; }

    public IReadOnlyList<RanchPortfolioEntry> Portfolio { get; init; } = [];

    public int SnapshotReads { get; private set; }

    public Task<IReadOnlyList<RanchPortfolioEntry>> ReadPortfolioAsync(IReadOnlyList<Guid> ranchIds, CancellationToken cancellationToken) =>
        Task.FromResult(Portfolio);

    public Task<AnalyticsSnapshot> ReadAsync(Guid ranchId, DateRange period, CancellationToken cancellationToken)
    {
        SnapshotReads++;
        return Task.FromResult(Snapshot!);
    }
}

internal sealed class FakeAnalyticsEntitlements(bool included) : IAnalyticsEntitlements
{
    public Task<bool> CanExportAsync(Guid ranchId, CancellationToken cancellationToken) => Task.FromResult(included);
}
