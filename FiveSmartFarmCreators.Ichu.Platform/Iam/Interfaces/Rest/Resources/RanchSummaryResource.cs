namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;

public sealed record RanchSummaryResource(Guid RanchId, string Name, string Region, string District);
