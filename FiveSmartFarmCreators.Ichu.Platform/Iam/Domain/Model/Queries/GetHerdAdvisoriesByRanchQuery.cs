namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Queries;

/// <summary>
///     Requests advisory history for a ranch visible to the current account.
/// </summary>
/// <param name="RanchId">
///     The ranch whose advisory history is requested.
/// </param>
public sealed record GetHerdAdvisoriesByRanchQuery(Guid RanchId);
