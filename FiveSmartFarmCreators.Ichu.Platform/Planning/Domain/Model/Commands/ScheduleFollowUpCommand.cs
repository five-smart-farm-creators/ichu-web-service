using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Commands;

public sealed record ScheduleFollowUpCommand(Guid RanchId, PlannedActivityId ActivityId, DateTimeOffset DueAt);
