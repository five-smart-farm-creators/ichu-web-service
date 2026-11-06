using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Queries;

/// <summary>
///     Requests an access decision for an account, ranch, and scope.
/// </summary>
/// <param name="AccountId">
///     The trusted account identifier being evaluated.
/// </param>
/// <param name="RanchId">
///     The ranch for which access is checked.
/// </param>
/// <param name="Scope">
///     The operation the account wants to perform.
/// </param>
public sealed record CheckRanchAccessQuery(Guid AccountId, Guid RanchId, AccessScope Scope);
