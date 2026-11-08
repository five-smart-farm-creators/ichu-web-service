using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Rest.Resources;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Rest.Transform;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Rest;

[ApiController]
[Authorize]
[Route("api/v1/ranches/{ranchId:guid}/devices/{deviceId:guid}")]
[Produces("application/json")]
[Tags("Device Assignments")]
public class DeviceAssignmentsController(
    IDeviceCommandService commandService,
    IDeviceQueryService queryService,
    TimeProvider timeProvider,
    IOptions<DeviceHealthOptions> healthOptions,
    IStringLocalizer<IoTAssetsMessages> localizer) : ControllerBase
{
    private readonly int? _lowBatteryThreshold = healthOptions.Value.LowBatteryThreshold;

    [HttpPost("assignments")]
    [EndpointSummary("Assign a collar to cattle or a controller to a water trough")]
    [ProducesResponseType(typeof(DeviceResource), StatusCodes.Status201Created)]
    public async Task<IActionResult> AssignDevice(Guid ranchId, Guid deviceId, AssignDeviceResource resource,
        CancellationToken cancellationToken)
    {
        var result = await commandService.HandleAsync(
            AssignDeviceCommandFromResourceAssembler.ToCommandFromResource(ranchId, deviceId, resource), cancellationToken);
        return IoTAssetsActionResultAssembler.ToActionResult(this, result, localizer,
            device => CreatedAtAction(nameof(GetDeviceAssignments), new { ranchId, deviceId },
                DeviceResourceFromEntityAssembler.ToResourceFromEntity(device, timeProvider.GetUtcNow(), _lowBatteryThreshold)));
    }

    [HttpGet("assignments")]
    [EndpointSummary("Get the retained device assignment history")]
    [ProducesResponseType(typeof(IEnumerable<DeviceAssignmentResource>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeviceAssignments(Guid ranchId, Guid deviceId, CancellationToken cancellationToken)
    {
        var result = await queryService.HandleAsync(new GetDeviceAssignmentsQuery(ranchId, new DeviceId(deviceId)), cancellationToken);
        return IoTAssetsActionResultAssembler.ToActionResult(this, result, localizer,
            assignments => Ok(assignments.Select(DeviceAssignmentResourceFromEntityAssembler.ToResourceFromEntity).ToArray()));
    }

    [HttpPost("releases")]
    [EndpointSummary("Release a device while retaining its assignment history")]
    [ProducesResponseType(typeof(DeviceResource), StatusCodes.Status200OK)]
    public async Task<IActionResult> ReleaseDevice(Guid ranchId, Guid deviceId, ReleaseDeviceResource resource,
        CancellationToken cancellationToken)
    {
        var result = await commandService.HandleAsync(
            ReleaseDeviceCommandFromResourceAssembler.ToCommandFromResource(ranchId, deviceId, resource), cancellationToken);
        return IoTAssetsActionResultAssembler.ToActionResult(this, result, localizer,
            device => Ok(DeviceResourceFromEntityAssembler.ToResourceFromEntity(device, timeProvider.GetUtcNow(), _lowBatteryThreshold)));
    }
}
