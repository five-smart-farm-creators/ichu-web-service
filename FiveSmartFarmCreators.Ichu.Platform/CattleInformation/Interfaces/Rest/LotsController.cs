using System.Net.Mime;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Rest.Resources;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Rest.Transform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Rest;

[ApiController]
[Authorize]
[Route("api/v1/ranches/{ranchId:guid}/lots")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Cattle Lots")]
[TypeFilter(typeof(CattleInformationExceptionFilter))]
public sealed class LotsController : ControllerBase
{
    private readonly ILotCommandService _commandService;
    private readonly ILotQueryService _queryService;

    public LotsController(ILotCommandService commandService, ILotQueryService queryService)
    {
        _commandService = commandService;
        _queryService = queryService;
    }

    [HttpPost]
    [EndpointSummary("Create a cattle lot")]
    [ProducesResponseType<LotResource>(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateLotAsync(Guid ranchId, CreateLotResource resource,
        CancellationToken cancellationToken)
    {
        var lot = await _commandService.HandleAsync(
            new CreateLotCommand(ranchId, resource.Name, resource.Criterion), cancellationToken);
        return Created($"/api/v1/ranches/{ranchId}/lots", LotResourceFromEntityAssembler.ToResourceFromEntity(lot));
    }

    [HttpGet]
    [EndpointSummary("List lots in an authorized ranch")]
    [ProducesResponseType<IReadOnlyList<LotResource>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLotsByRanchAsync(Guid ranchId, CancellationToken cancellationToken)
    {
        var lots = await _queryService.HandleAsync(new GetLotsByRanchQuery(ranchId), cancellationToken);
        return Ok(lots.Select(LotResourceFromEntityAssembler.ToResourceFromEntity).ToList());
    }

    [HttpPut("{lotId:guid}/cattle/{cattleId:guid}")]
    [EndpointSummary("Assign active cattle to a lot in the same ranch")]
    [ProducesResponseType<LotResource>(StatusCodes.Status200OK)]
    public async Task<IActionResult> AssignCattleToLotAsync(Guid ranchId, Guid lotId, Guid cattleId,
        AssignCattleToLotResource resource, CancellationToken cancellationToken)
    {
        var command = new AssignCattleToLotCommand(ranchId, lotId, cattleId, resource.ExpectedVersion);
        var lot = await _commandService.HandleAsync(command, cancellationToken);
        return Ok(LotResourceFromEntityAssembler.ToResourceFromEntity(lot));
    }

    [HttpDelete("{lotId:guid}/cattle/{cattleId:guid}")]
    [EndpointSummary("Remove cattle from a lot without deleting its identity")]
    [ProducesResponseType<LotResource>(StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveCattleFromLotAsync(Guid ranchId, Guid lotId, Guid cattleId,
        [FromQuery] Guid expectedVersion, CancellationToken cancellationToken)
    {
        var command = new RemoveCattleFromLotCommand(ranchId, lotId, cattleId, expectedVersion);
        var lot = await _commandService.HandleAsync(command, cancellationToken);
        return Ok(LotResourceFromEntityAssembler.ToResourceFromEntity(lot));
    }
}
