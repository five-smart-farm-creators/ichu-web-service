using System.Net.Mime;
using System.Text.RegularExpressions;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest;

[ApiController]
[Route("api/v1/water-controllers")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Water monitoring")]
public sealed class WaterControllersController(
    IWaterTroughReadingCommandService readingCommandService,
    IWaterTroughMonitoringQueryService monitoringQueryService,
    IStringLocalizer<OperationsMonitoringMessages> localizer) : ControllerBase
{
    private static readonly Regex ExplicitOffset = new(@"(?:Z|[+-]\d{2}:\d{2})$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    [HttpPost("{controllerId:guid}/readings")]
    [AllowAnonymous]
    [EndpointSummary("Receive a water controller reading")]
    [EndpointDescription("Requires X-Gateway-Key for the Edge Gateway. Required fields: readingId (stable UUID), temperature (decimal), temperatureUnit (Celsius), isValid (boolean), heaterState (Off or On), and capturedAt (ISO-8601 with explicit UTC offset and microsecond precision). events is optional and must be omitted or empty because its full schema is not defined. A matching retry returns 200; changed content under the same controllerId and readingId returns 409. Errors: 400 OperationsMonitoring.WaterReadingInvalid, CaptureTimeAhead, or WaterReadingEventsUnsupported; 401 GatewayUnauthenticated; 403 WaterControllerGatewayAccessDenied; 404 WaterControllerNotFound; 409 WaterReadingIdentifierConflict or WaterReadingConcurrentlyStored.")]
    [ProducesResponseType(typeof(WaterTroughReadingResource), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(WaterTroughReadingResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ReceiveReading(Guid controllerId, WaterControllerReadingResource resource,
        CancellationToken cancellationToken)
    {
        if (resource.Events is { } events && events.ValueKind != System.Text.Json.JsonValueKind.Null
            && !(events.ValueKind == System.Text.Json.JsonValueKind.Array && events.GetArrayLength() == 0))
        {
            return OperationsMonitoringActionResultAssembler.ToProblem(this,
                FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors.OperationsMonitoringErrors.WaterReadingEventsUnsupported,
                localizer);
        }

        var command = new RegisterWaterControllerReadingCommand(Guid.Empty, Guid.Empty, controllerId, Guid.Empty,
            resource.ReadingId, resource.Temperature, resource.TemperatureUnit, resource.IsValid,
            resource.HeaterState, resource.CapturedAt, default);
        var result = await readingCommandService.Handle(controllerId, command, cancellationToken);
        return OperationsMonitoringActionResultAssembler.ToActionResult(this, result, localizer, receipt =>
        {
            var response = WaterTroughReadingResourceFromEntityAssembler.ToResourceFromEntity(receipt.Reading);
            return receipt.WasCreated ? StatusCode(StatusCodes.Status201Created, response) : Ok(response);
        });
    }

    [HttpGet("{controllerId:guid}/readings")]
    [Authorize]
    [EndpointSummary("List water controller readings")]
    [EndpointDescription("Requires a user bearer token and Read access to the controller's ranch. from and to are required ISO-8601 instants with explicit offsets and microsecond precision; the half-open interval [from,to) is normalized to UTC. Results are ordered by capturedAt then readingId ascending. page starts at 1; pageSize defaults to 100 and is capped at 500. Each item includes stable id and readingId, controller/trough/ranch/gateway IDs, temperature in Celsius, validity, heater state, capturedAt, server receivedAt, and fixed origin EdgeGateway. Errors: 400 PeriodStartNotBeforeEnd, WaterReadingPeriodOffsetRequired, WaterReadingPeriodPrecisionInvalid, or InvalidPage; 401 Unauthenticated; 404 WaterControllerNotFound (including other-ranch controllers).")]
    [ProducesResponseType(typeof(WaterControllerReadingsPageResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReadings(Guid controllerId,
        [FromQuery, BindRequired] DateTimeOffset from,
        [FromQuery, BindRequired] DateTimeOffset to,
        CancellationToken cancellationToken,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 100)
    {
        if (!ExplicitOffset.IsMatch(Request.Query["from"].ToString())
            || !ExplicitOffset.IsMatch(Request.Query["to"].ToString()))
        {
            return OperationsMonitoringActionResultAssembler.ToProblem(this,
                FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors.OperationsMonitoringErrors.WaterReadingPeriodOffsetRequired,
                localizer);
        }

        var period = TimePeriodFromRequestAssembler.ToPeriodFromRequest(from, to);
        if (period.IsFailure)
        {
            return OperationsMonitoringActionResultAssembler.ToProblem(this, period.DomainError, localizer);
        }

        var validPeriod = period.Value!;
        if (from.UtcTicks % 10 != 0 || to.UtcTicks % 10 != 0)
        {
            return OperationsMonitoringActionResultAssembler.ToProblem(this,
                FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors.OperationsMonitoringErrors.WaterReadingPeriodPrecisionInvalid,
                localizer);
        }

        var result = await monitoringQueryService.Handle(
            new GetWaterControllerReadingsQuery(controllerId, validPeriod, page, pageSize), cancellationToken);
        return OperationsMonitoringActionResultAssembler.ToActionResult(this, result, localizer, readings =>
            Ok(new WaterControllerReadingsPageResource(controllerId, validPeriod.From, validPeriod.To,
                readings.Page, readings.PageSize, readings.TotalCount,
                readings.Items.Select(WaterTroughReadingResourceFromEntityAssembler.ToResourceFromEntity).ToArray())));
    }
}
