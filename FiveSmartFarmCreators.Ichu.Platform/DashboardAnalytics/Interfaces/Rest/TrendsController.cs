using System.Net.Mime;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Resources;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Transform;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest;

[ApiController]
[Authorize(AuthenticationSchemes = "Firebase")]
[Route("api/v1/ranches/{ranchId:guid}/trends")]
[Produces(MediaTypeNames.Application.Json)]
public sealed class TrendsController(
    IAnalyticsQueryService service,
    IStringLocalizer<DashboardAnalyticsMessages> localizer) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("Get the trend of a lot metric")]
    [EndpointDescription("Returns the equally weighted lot average of the metric and the animals whose averages deviate beyond the threshold.")]
    [ProducesResponseType<TrendSeriesResource>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Get(Guid ranchId, Guid lotId, DateTimeOffset from, DateTimeOffset to,
        string metric, double deviation, CancellationToken cancellationToken)
    {
        var period = DateRange.Create(from, to);
        if (period.IsFailure)
        {
            return DashboardAnalyticsActionResultAssembler.ToProblem(this, period.DomainError, localizer);
        }

        var result = await service.HandleAsync(new GetLotTrendQuery(ranchId, lotId, period.Value!, metric, deviation), cancellationToken);
        return result.IsFailure
            ? DashboardAnalyticsActionResultAssembler.ToProblem(this, result.DomainError, localizer)
            : Ok(TrendSeriesResourceFromValueObjectAssembler.ToResourceFromValueObject(result.Value!));
    }

    [HttpGet("animals/{animalId:guid}/comparison")]
    [EndpointSummary("Compare an animal with its lot")]
    [EndpointDescription("Compares the average of the metric for one animal with its lot, when at least five animals have valid readings.")]
    [ProducesResponseType<AnimalComparisonResource>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Compare(Guid ranchId, Guid animalId, Guid lotId, DateTimeOffset from,
        DateTimeOffset to, string metric, double deviation, CancellationToken cancellationToken)
    {
        var period = DateRange.Create(from, to);
        if (period.IsFailure)
        {
            return DashboardAnalyticsActionResultAssembler.ToProblem(this, period.DomainError, localizer);
        }

        var result = await service.HandleAsync(
            new GetAnimalComparisonQuery(ranchId, lotId, animalId, period.Value!, metric, deviation), cancellationToken);
        return result.IsFailure
            ? DashboardAnalyticsActionResultAssembler.ToProblem(this, result.DomainError, localizer)
            : Ok(AnimalComparisonResourceFromValueObjectAssembler.ToResourceFromValueObject(result.Value!));
    }
}
