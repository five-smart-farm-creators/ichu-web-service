namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Commands;

/// <summary>
/// Requests scheduling a follow-up for a planned activity.
/// </summary>
/// <param name="ActivityId">The planned activity identifier.</param>
/// <param name="DueAt">The due UTC instant.</param>
public sealed record ScheduleFollowUpCommand(Guid ActivityId, DateTimeOffset DueAt);
