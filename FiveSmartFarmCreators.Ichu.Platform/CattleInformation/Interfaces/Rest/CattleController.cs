using System.Net.Mime;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Rest.Resources;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Rest.Transform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Rest;

[ApiController]
[Authorize]
[Route("api/v1/ranches/{ranchId:guid}/cattle")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Cattle Information")]
[TypeFilter(typeof(CattleInformationExceptionFilter))]
public sealed class CattleController : ControllerBase
{
    private readonly ICattleCommandService _commandService;
    private readonly ICattleQueryService _queryService;

    public CattleController(ICattleCommandService commandService, ICattleQueryService queryService)
    {
        _commandService = commandService;
        _queryService = queryService;
    }

    [HttpPost]
    [EndpointSummary("Register cattle in a ranch")]
    [ProducesResponseType<CattleResource>(StatusCodes.Status201Created)]
    public async Task<IActionResult> RegisterCattleAsync(Guid ranchId, RegisterCattleResource resource,
        CancellationToken cancellationToken)
    {
        var command = new RegisterCattleCommand(
            ranchId, resource.EarTag, resource.Breed, resource.Sex, resource.BirthDate, resource.LifeStage);
        var cattle = await _commandService.HandleAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetCattleByIdAsync), new { ranchId, cattleId = cattle.Id },
            CattleResourceFromEntityAssembler.ToResourceFromEntity(cattle));
    }

    [HttpGet("{cattleId:guid}")]
    [ActionName(nameof(GetCattleByIdAsync))]
    [EndpointSummary("Get the authorized cattle profile")]
    [ProducesResponseType<CattleProfileResource>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCattleByIdAsync(Guid ranchId, Guid cattleId,
        CancellationToken cancellationToken)
    {
        var profile = await _queryService.HandleAsync(new GetCattleByIdQuery(ranchId, cattleId), cancellationToken);
        return Ok(CattleProfileResourceFromModelAssembler.ToResourceFromModel(profile));
    }

    [HttpGet]
    [EndpointSummary("Search cattle in an authorized ranch")]
    [ProducesResponseType<IReadOnlyList<CattleResource>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchCattleAsync(Guid ranchId, CancellationToken cancellationToken,
        [FromQuery] string? earTag = null, [FromQuery] CattleStatus? status = null,
        [FromQuery] Guid? lotId = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var query = new SearchCattleQuery(ranchId, earTag, status, lotId, page, pageSize);
        var cattle = await _queryService.HandleAsync(query, cancellationToken);
        return Ok(cattle.Select(CattleResourceFromEntityAssembler.ToResourceFromEntity).ToList());
    }

    [HttpPut("{cattleId:guid}")]
    [EndpointSummary("Update cattle identification and productive details")]
    [ProducesResponseType<CattleResource>(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateCattleAsync(Guid ranchId, Guid cattleId, UpdateCattleResource resource,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCattleCommand(
            ranchId, cattleId, resource.EarTag, resource.Breed, resource.Sex, resource.BirthDate,
            resource.ExpectedVersion);
        var cattle = await _commandService.HandleAsync(command, cancellationToken);
        return Ok(CattleResourceFromEntityAssembler.ToResourceFromEntity(cattle));
    }

    [HttpPatch("{cattleId:guid}/life-stage")]
    [EndpointSummary("Change the productive stage while retaining its history")]
    [ProducesResponseType<CattleResource>(StatusCodes.Status200OK)]
    public async Task<IActionResult> ChangeLifeStageAsync(Guid ranchId, Guid cattleId, ChangeLifeStageResource resource,
        CancellationToken cancellationToken)
    {
        var command = new ChangeLifeStageCommand(
            ranchId, cattleId, resource.LifeStage, resource.ValidFrom, resource.ExpectedVersion);
        var cattle = await _commandService.HandleAsync(command, cancellationToken);
        return Ok(CattleResourceFromEntityAssembler.ToResourceFromEntity(cattle));
    }

    [HttpPost("{cattleId:guid}/deactivations")]
    [EndpointSummary("Deactivate cattle and retain the exit reason")]
    [ProducesResponseType<CattleResource>(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeactivateCattleAsync(Guid ranchId, Guid cattleId, DeactivateCattleResource resource,
        CancellationToken cancellationToken)
    {
        var command = new DeactivateCattleCommand(
            ranchId, cattleId, resource.Reason, resource.ExitDate, resource.ExpectedVersion);
        var cattle = await _commandService.HandleAsync(command, cancellationToken);
        return Ok(CattleResourceFromEntityAssembler.ToResourceFromEntity(cattle));
    }
}
