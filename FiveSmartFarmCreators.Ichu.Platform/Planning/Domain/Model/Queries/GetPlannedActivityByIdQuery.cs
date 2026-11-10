using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Queries;

public sealed record GetPlannedActivityByIdQuery(Guid RanchId, PlannedActivityId ActivityId);
