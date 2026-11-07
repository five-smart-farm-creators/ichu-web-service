using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Transform;
using Microsoft.AspNetCore.Mvc;

namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest;

/// <summary>
///     Exposes lot trends and animal comparisons for authorized ranches.
/// </summary>
[Route("api/v1/ranches/{ranchId:guid}/trends")]
public sealed class TrendsController(IAnalyticsQueryService service) : AnalyticsControllerBase
{
    /// <summary>
    ///     Returns the selected lot metric trend and absolute deviation outliers.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Get(Guid ranchId, Guid lotId, DateTimeOffset from, DateTimeOffset to,
        string metric, double deviation, CancellationToken cancellationToken) =>
        Ok(TrendSeriesResourceFromValueObjectAssembler.ToResourceFromValueObject(
            await service.HandleAsync(new GetLotTrendQuery(ranchId, lotId, new DateRange(from, to), metric, deviation), cancellationToken)));

    /// <summary>
    ///     Compares an animal against its lot reference using the selected metric.
    /// </summary>
    [HttpGet("animals/{animalId:guid}/comparison")]
    public async Task<IActionResult> Compare(Guid ranchId, Guid animalId, Guid lotId, DateTimeOffset from,
        DateTimeOffset to, string metric, double deviation, CancellationToken cancellationToken) =>
        Ok(AnimalComparisonResourceFromValueObjectAssembler.ToResourceFromValueObject(
            await service.HandleAsync(new GetAnimalComparisonQuery(ranchId, lotId, animalId, new DateRange(from, to), metric, deviation), cancellationToken)));
}
