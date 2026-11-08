using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Rest.Resources;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Rest.Transform;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Rest;

[ApiController]
[Authorize]
[Route("api/v1/ranches/{ranchId:guid}/edge-gateways")]
[Produces("application/json")]
[Tags("IoT Edge Gateways")]
public class RanchEdgeGatewaysController(
    IEdgeGatewayCommandService commandService,
    IEdgeGatewayQueryService queryService,
    IStringLocalizer<IoTAssetsMessages> localizer) : ControllerBase
{
    [HttpPost]
    [EndpointSummary("Register an edge gateway")]
    [EndpointDescription("Returns the gateway key of the form {gatewayId}.{secret} only once. The service keeps a digest of the secret, so a lost key cannot be recovered.")]
    [ProducesResponseType(typeof(EdgeGatewayRegistrationResource), StatusCodes.Status201Created)]
    public async Task<IActionResult> RegisterEdgeGateway(Guid ranchId, RegisterEdgeGatewayResource resource,
        CancellationToken cancellationToken)
    {
        var result = await commandService.HandleAsync(
            RegisterEdgeGatewayCommandFromResourceAssembler.ToCommandFromResource(ranchId, resource), cancellationToken);
        return IoTAssetsActionResultAssembler.ToActionResult(this, result, localizer,
            registration => CreatedAtAction(nameof(GetEdgeGatewayById), new { ranchId, gatewayId = registration.Gateway.Id.Value },
                EdgeGatewayResourceFromEntityAssembler.ToRegistrationResourceFromEntity(registration)));
    }

    [HttpGet("{gatewayId:guid}")]
    [EndpointSummary("Get an edge gateway in the authorized ranch")]
    [ProducesResponseType(typeof(EdgeGatewayResource), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEdgeGatewayById(Guid ranchId, Guid gatewayId, CancellationToken cancellationToken)
    {
        var result = await queryService.HandleAsync(new GetEdgeGatewayByIdQuery(ranchId, new EdgeGatewayId(gatewayId)), cancellationToken);
        return IoTAssetsActionResultAssembler.ToActionResult(this, result, localizer,
            gateway => Ok(EdgeGatewayResourceFromEntityAssembler.ToResourceFromEntity(gateway)));
    }

    [HttpGet]
    [EndpointSummary("Get the edge gateways in the authorized ranch")]
    [ProducesResponseType(typeof(IEnumerable<EdgeGatewayResource>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEdgeGateways(Guid ranchId, CancellationToken cancellationToken)
    {
        var result = await queryService.HandleAsync(new GetEdgeGatewaysByRanchQuery(ranchId), cancellationToken);
        return IoTAssetsActionResultAssembler.ToActionResult(this, result, localizer,
            gateways => Ok(gateways.Select(EdgeGatewayResourceFromEntityAssembler.ToResourceFromEntity).ToArray()));
    }

    [HttpPost("{gatewayId:guid}/revocations")]
    [EndpointSummary("Revoke an edge gateway so that its key stops authenticating")]
    [ProducesResponseType(typeof(EdgeGatewayResource), StatusCodes.Status200OK)]
    public async Task<IActionResult> RevokeEdgeGateway(Guid ranchId, Guid gatewayId, CancellationToken cancellationToken)
    {
        var result = await commandService.HandleAsync(new RevokeEdgeGatewayCommand(ranchId, new EdgeGatewayId(gatewayId)),
            cancellationToken);
        return IoTAssetsActionResultAssembler.ToActionResult(this, result, localizer,
            gateway => Ok(EdgeGatewayResourceFromEntityAssembler.ToResourceFromEntity(gateway)));
    }
}
