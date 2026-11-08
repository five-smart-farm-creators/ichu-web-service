using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Interfaces.Acl;

namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Application.Internal.OutboundServices;

/// <summary>
///     Gates report exports on the plan feature that the subscription context defines, so the plan catalog stays the only source of entitlements.
/// </summary>
public sealed class SubscriptionPlansAnalyticsEntitlements(ISubscriptionPlansContextFacade subscriptionPlans) : IAnalyticsEntitlements
{
    /// <inheritdoc />
    public Task<bool> CanExportAsync(Guid ranchId, CancellationToken cancellationToken) =>
        subscriptionPlans.IncludesFeatureAsync(ranchId, nameof(FeatureCode.ReportExport), cancellationToken);
}
