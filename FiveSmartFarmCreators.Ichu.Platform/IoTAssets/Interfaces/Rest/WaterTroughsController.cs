using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Rest.Resources;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Rest.Transform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Rest;

[ApiController]
[Authorize]
[Route("api/v1/ranches/{ranchId:guid}/water-troughs")]
[Produces("application/json")]
[Tags("Water Troughs")]
public class WaterTroughsController(IWaterTroughCommandService commandService, IWaterTroughQueryService queryService) : ControllerBase
{
    [HttpPost]
    [EndpointSummary("Register a water trough without consuming the collar quota")]
    [ProducesResponseType(typeof(WaterTroughResource), StatusCodes.Status201Created)]
    public async Task<IActionResult> RegisterWaterTrough(Guid ranchId, RegisterWaterTroughResource resource,
        CancellationToken cancellationToken)
    {
        var result = await commandService.HandleAsync(
            RegisterWaterTroughCommandFromResourceAssembler.ToCommandFromResource(ranchId, resource), cancellationToken);
        return IoTAssetsActionResultAssembler.ToActionResult(this, result,
            trough => CreatedAtAction(nameof(GetWaterTroughById), new { ranchId, waterTroughId = trough.Id },
                WaterTroughResourceFromEntityAssembler.ToResourceFromEntity(trough)));
    }

    [HttpGet("{waterTroughId:guid}")]
    [EndpointSummary("Get a water trough and its configured temperature range")]
    [ProducesResponseType(typeof(WaterTroughResource), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWaterTroughById(Guid ranchId, Guid waterTroughId, CancellationToken cancellationToken)
    {
        var result = await queryService.HandleAsync(new GetWaterTroughByIdQuery(ranchId, waterTroughId), cancellationToken);
        return IoTAssetsActionResultAssembler.ToActionResult(this, result,
            trough => Ok(WaterTroughResourceFromEntityAssembler.ToResourceFromEntity(trough)));
    }

    [HttpGet]
    [EndpointSummary("Get water troughs in the authorized ranch")]
    [ProducesResponseType(typeof(IEnumerable<WaterTroughResource>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWaterTroughs(Guid ranchId, CancellationToken cancellationToken)
    {
        var result = await queryService.HandleAsync(new GetWaterTroughsQuery(ranchId), cancellationToken);
        return IoTAssetsActionResultAssembler.ToActionResult(this, result,
            troughs => Ok(troughs.Select(WaterTroughResourceFromEntityAssembler.ToResourceFromEntity).ToArray()));
    }

    [HttpPut("{waterTroughId:guid}/temperature-range")]
    [EndpointSummary("Save the desired water temperature range in degrees Celsius")]
    [ProducesResponseType(typeof(WaterTroughResource), StatusCodes.Status200OK)]
    public async Task<IActionResult> ConfigureTemperatureRange(Guid ranchId, Guid waterTroughId,
        ConfigureWaterTroughTemperatureResource resource, CancellationToken cancellationToken)
    {
        var result = await commandService.HandleAsync(
            ConfigureWaterTroughTemperatureCommandFromResourceAssembler.ToCommandFromResource(ranchId, waterTroughId, resource),
            cancellationToken);
        return IoTAssetsActionResultAssembler.ToActionResult(this, result,
            trough => Ok(WaterTroughResourceFromEntityAssembler.ToResourceFromEntity(trough)));
    }
}
