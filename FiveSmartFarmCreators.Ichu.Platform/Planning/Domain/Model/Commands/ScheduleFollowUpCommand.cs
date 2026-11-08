using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Commands;

public sealed record ScheduleFollowUpCommand(PlannedActivityId ActivityId, DateTimeOffset DueAt);
