namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Resources;

/// <summary>
/// Request body for scheduling a follow-up.
/// </summary>
/// <param name="DueAt">The due UTC instant in ISO 8601 format.</param>
public sealed record ScheduleFollowUpResource(DateTimeOffset DueAt);
