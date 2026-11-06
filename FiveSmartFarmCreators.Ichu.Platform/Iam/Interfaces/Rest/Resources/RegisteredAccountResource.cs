namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;

/// <summary>
///     Contains the created account and its optional ranch identifier.
/// </summary>
public sealed record RegisteredAccountResource(AccountResource Account, Guid? RanchId);
