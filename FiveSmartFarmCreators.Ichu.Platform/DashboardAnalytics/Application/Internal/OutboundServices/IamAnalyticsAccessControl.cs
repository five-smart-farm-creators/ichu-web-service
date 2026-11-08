using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Acl;

namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Application.Internal.OutboundServices;

/// <summary>
///     Reads ranch access from IAM with the read scope, so analytics honors the same grants as the other read endpoints.
/// </summary>
public sealed class IamAnalyticsAccessControl(IIamContextFacade iamContextFacade) : IAnalyticsAccessControl
{
    /// <inheritdoc />
    public Task<Guid?> GetCurrentAccountIdAsync(CancellationToken cancellationToken) =>
        iamContextFacade.FetchCurrentAccountIdAsync(cancellationToken);

    /// <inheritdoc />
    public Task<bool> CanReadRanchAsync(Guid accountId, Guid ranchId, CancellationToken cancellationToken) =>
        iamContextFacade.CanAccessRanchAsync(accountId, ranchId, AccessScope.Read, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Guid>> GetAccessibleRanchIdsAsync(Guid accountId, CancellationToken cancellationToken) =>
        (await iamContextFacade.GetAccessibleRanchesAsync(accountId, cancellationToken))
        .Select(access => access.RanchId)
        .ToList();
}
