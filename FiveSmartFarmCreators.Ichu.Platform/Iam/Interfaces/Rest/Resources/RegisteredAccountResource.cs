namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;

public sealed record RegisteredAccountResource(AccountResource Account, Guid? RanchId);
