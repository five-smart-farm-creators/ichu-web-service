namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Commands;

/// <summary>
///     Carries a veterinarian's request to access a ranch.
/// </summary>
/// <param name="RanchId">
///     The ranch for which access is requested.
/// </param>
/// <param name="Reason">
///     The veterinarian's reason for requesting access.
/// </param>
public sealed record RequestHerdAdvisoryCommand(Guid RanchId, string Reason);
