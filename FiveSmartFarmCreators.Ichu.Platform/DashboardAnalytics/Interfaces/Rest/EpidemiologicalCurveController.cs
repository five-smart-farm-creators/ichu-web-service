using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Transform;
using Microsoft.AspNetCore.Mvc;

namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest;

/// <summary>
///     Exposes daily case progression and observation limits for authorized ranches.
/// </summary>
[Route("api/v1/ranches/{ranchId:guid}/epidemiological-curves")]
public sealed class EpidemiologicalCurveController(IAnalyticsQueryService service) : AnalyticsControllerBase
{
    /// <summary>
    ///     Returns daily case onset counts and the available incubation assessment.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Get(Guid ranchId, DateTimeOffset from, DateTimeOffset to,
        string condition, CancellationToken cancellationToken) =>
        Ok(EpidemiologicalCurveResourceFromValueObjectAssembler.ToResourceFromValueObject(
            await service.HandleAsync(new GetEpidemiologicalCurveQuery(ranchId, new DateRange(from, to), condition), cancellationToken)));
}
