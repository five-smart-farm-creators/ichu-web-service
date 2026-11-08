using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Acl;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices;

/// <summary>
///     Implements <see cref="IMonitoringAccessControl"/> with the IAM context facade, translating each monitoring permission to the IAM access scope that grants it.
/// </summary>
public class IamOperationsMonitoringAccessControl(IIamContextFacade iamContextFacade) : IMonitoringAccessControl
{
    /// <inheritdoc />
    public Task<Guid?> GetCurrentAccountIdAsync(CancellationToken cancellationToken) =>
        iamContextFacade.FetchCurrentAccountIdAsync(cancellationToken);

    /// <inheritdoc />
    public Task<bool> CanAccessRanchAsync(Guid accountId, Guid ranchId, MonitoringPermission permission, CancellationToken cancellationToken) =>
        iamContextFacade.CanAccessRanchAsync(accountId, ranchId, ToAccessScope(permission), cancellationToken);

    private static AccessScope ToAccessScope(MonitoringPermission permission)
    {
        // Field events and rules change ranch data, so they need the manage scope that only the ranch manager holds.
        return permission switch
        {
            MonitoringPermission.ReadMonitoring => AccessScope.Read,
            MonitoringPermission.AttendAlerts => AccessScope.ClinicalWrite,
            MonitoringPermission.RecordClinicalInterventions => AccessScope.ClinicalWrite,
            MonitoringPermission.RecordFieldEvents => AccessScope.Manage,
            MonitoringPermission.ManageRules => AccessScope.Manage,
            // Every permission is mapped above, so an unmapped value is a programming error.
            _ => throw new ArgumentOutOfRangeException(nameof(permission), permission, null)
        };
    }
}
