namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Resources;

/// <summary>
/// Response resource for a scheduled follow-up.
/// </summary>
/// <param name="Id">The follow-up identifier.</param>
/// <param name="ActivityId">The related planned activity identifier.</param>
/// <param name="DueAt">The due UTC instant.</param>
public sealed record FollowUpResource(Guid Id, Guid ActivityId, DateTimeOffset DueAt);
