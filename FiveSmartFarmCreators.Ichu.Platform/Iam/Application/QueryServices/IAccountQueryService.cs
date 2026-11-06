using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Acl;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Application.QueryServices;

/// <summary>Defines the AccountQueryService application contract.</summary>
public interface IAccountQueryService
{
    /// <summary>Returns the current account from the verified request identity.</summary>
    Task<UserAccount> HandleAsync(GetCurrentAccountQuery query, CancellationToken cancellationToken = default);

    /// <summary>Lists ranches currently accessible to the authenticated account.</summary>
    Task<IReadOnlyList<RanchAccess>> HandleAsync(GetCurrentRanchesQuery query, CancellationToken cancellationToken = default);
}
