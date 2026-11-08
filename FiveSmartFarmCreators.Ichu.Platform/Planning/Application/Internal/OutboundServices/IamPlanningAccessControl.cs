using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Acl;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Internal.OutboundServices;

/// <summary>
/// Implements <see cref="IPlanningAccessControl"/> with the IAM context facade, translating each planning permission to the IAM access scope that grants it.
/// </summary>
public sealed class IamPlanningAccessControl(IIamContextFacade iamContextFacade) : IPlanningAccessControl
{
    /// <inheritdoc />
    public Task<Guid?> GetCurrentAccountIdAsync(CancellationToken cancellationToken) =>
        iamContextFacade.FetchCurrentAccountIdAsync(cancellationToken);

    /// <inheritdoc />
    public Task<bool> CanAccessRanchAsync(Guid accountId, Guid ranchId, PlanningPermission permission, CancellationToken cancellationToken) =>
        iamContextFacade.CanAccessRanchAsync(accountId, ranchId, ToAccessScope(permission), cancellationToken);

    private static AccessScope ToAccessScope(PlanningPermission permission)
    {
        // Campaigns and follow-ups need the manage scope that only the ranch manager holds, while clinical records also reach veterinarians with a clinical-write advisory.
        return permission switch
        {
            PlanningPermission.ReadPlanning => AccessScope.Read,
            PlanningPermission.ManagePlanning => AccessScope.Manage,
            PlanningPermission.RecordClinicalPlanning => AccessScope.ClinicalWrite,
            // Every permission is mapped above, so an unmapped value is a programming error.
            _ => throw new ArgumentOutOfRangeException(nameof(permission), permission, null)
        };
    }
}
