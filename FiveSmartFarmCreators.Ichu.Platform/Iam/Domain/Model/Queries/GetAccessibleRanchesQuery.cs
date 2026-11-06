namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Queries;

/// <summary>
///     Requests all effective ranch access for an account.
/// </summary>
/// <param name="AccountId">
///     The trusted account identifier being queried.
/// </param>
public sealed record GetAccessibleRanchesQuery(Guid AccountId);
