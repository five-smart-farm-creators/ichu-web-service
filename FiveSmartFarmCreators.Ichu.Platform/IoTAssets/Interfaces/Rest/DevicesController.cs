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
[Route("api/v1/ranches/{ranchId:guid}/devices")]
[Produces("application/json")]
[Tags("IoT Devices")]
public class DevicesController(
    IDeviceCommandService commandService,
    IDeviceQueryService queryService,
    TimeProvider timeProvider,
    IOptions<DeviceHealthOptions> healthOptions,
    IStringLocalizer<IoTAssetsMessages> localizer) : ControllerBase
{
    private readonly int? _lowBatteryThreshold = healthOptions.Value.LowBatteryThreshold;

    [HttpPost]
    [EndpointSummary("Register a device")]
    [ProducesResponseType(typeof(DeviceResource), StatusCodes.Status201Created)]
    public async Task<IActionResult> RegisterDevice(Guid ranchId, RegisterDeviceResource resource, CancellationToken cancellationToken)
    {
        var result = await commandService.HandleAsync(
            RegisterDeviceCommandFromResourceAssembler.ToCommandFromResource(ranchId, resource), cancellationToken);
        return IoTAssetsActionResultAssembler.ToActionResult(this, result, localizer,
            device => CreatedAtAction(nameof(GetDeviceById), new { ranchId, deviceId = device.Id.Value },
                DeviceResourceFromEntityAssembler.ToResourceFromEntity(device, timeProvider.GetUtcNow(), _lowBatteryThreshold)));
    }

    [HttpGet("{deviceId:guid}")]
    [EndpointSummary("Get a device in the authorized ranch")]
    [ProducesResponseType(typeof(DeviceResource), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeviceById(Guid ranchId, Guid deviceId, CancellationToken cancellationToken)
    {
        var result = await queryService.HandleAsync(new GetDeviceByIdQuery(ranchId, new DeviceId(deviceId)), cancellationToken);
        return IoTAssetsActionResultAssembler.ToActionResult(this, result, localizer,
            device => Ok(DeviceResourceFromEntityAssembler.ToResourceFromEntity(device, timeProvider.GetUtcNow(), _lowBatteryThreshold)));
    }

    [HttpGet]
    [EndpointSummary("Get the device inventory")]
    [ProducesResponseType(typeof(IEnumerable<DeviceResource>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeviceInventory(Guid ranchId, CancellationToken cancellationToken)
    {
        var result = await queryService.HandleAsync(new GetDeviceInventoryQuery(ranchId), cancellationToken);
        var now = timeProvider.GetUtcNow();
        return IoTAssetsActionResultAssembler.ToActionResult(this, result, localizer,
            devices => Ok(devices.Select(device =>
                DeviceResourceFromEntityAssembler.ToResourceFromEntity(device, now, _lowBatteryThreshold)).ToArray()));
    }

    [HttpPut("{deviceId:guid}/maintenance")]
    [EndpointSummary("Change device maintenance status after releasing any assignment")]
    [ProducesResponseType(typeof(DeviceResource), StatusCodes.Status200OK)]
    public async Task<IActionResult> SetMaintenance(Guid ranchId, Guid deviceId, SetDeviceMaintenanceResource resource,
        CancellationToken cancellationToken)
    {
        var result = await commandService.HandleAsync(
            SetDeviceMaintenanceCommandFromResourceAssembler.ToCommandFromResource(ranchId, deviceId, resource), cancellationToken);
        return IoTAssetsActionResultAssembler.ToActionResult(this, result, localizer,
            device => Ok(DeviceResourceFromEntityAssembler.ToResourceFromEntity(device, timeProvider.GetUtcNow(), _lowBatteryThreshold)));
    }
}
