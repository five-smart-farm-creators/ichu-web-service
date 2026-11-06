namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;

/// <summary>Defines the RegisteredAccountResource REST representation.</summary>
public sealed record RegisteredAccountResource(AccountResource Account, Guid? RanchId);
