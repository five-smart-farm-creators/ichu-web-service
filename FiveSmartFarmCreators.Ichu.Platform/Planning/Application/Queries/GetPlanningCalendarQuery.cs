namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Queries;

/// <summary>
/// Requests planned activities in an inclusive UTC interval for one ranch.
/// </summary>
/// <param name="RanchId">The ranch identifier.</param>
/// <param name="From">The first inclusive UTC instant.</param>
/// <param name="To">The last inclusive UTC instant.</param>
public sealed record GetPlanningCalendarQuery(Guid RanchId, DateTimeOffset From, DateTimeOffset To);
