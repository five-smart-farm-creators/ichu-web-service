using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Commands;

public sealed record RecordApplicationCommand(
    Guid RanchId,
    PlannedActivityId ActivityId,
    Guid CattleId,
    DateTimeOffset AppliedAt,
    Guid? InterventionId);
