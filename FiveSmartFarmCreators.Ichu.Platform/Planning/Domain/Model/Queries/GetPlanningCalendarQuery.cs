namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Queries;

public sealed record GetPlanningCalendarQuery(Guid RanchId, DateTimeOffset From, DateTimeOffset To);
