namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Resources;

public sealed record FollowUpResource(Guid Id, Guid ActivityId, DateTimeOffset DueAt);
