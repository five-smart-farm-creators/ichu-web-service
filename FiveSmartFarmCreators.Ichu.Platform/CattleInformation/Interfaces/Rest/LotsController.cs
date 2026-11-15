using System.Net.Mime;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Rest.Resources;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Rest.Transform;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Rest;

/// <summary>
///     Exposes ranch-authorized management groups and membership changes without owning animal identity.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/ranches/{ranchId:guid}/lots")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Cattle Lots")]
public sealed class LotsController(
    ILotCommandService commandService,
    ILotQueryService queryService,
    IStringLocalizer<CattleInformationMessages> localizer) : ControllerBase
{
    /// <summary>
    ///     Creates a management group in the authorized ranch with an optional grouping criterion.
    /// </summary>
    [HttpPost]
    [EndpointSummary("Create a cattle lot")]
    [EndpointDescription("Creates exactly one empty cattle lot in the ranch identified by ranchId. Requires permission to manage that ranch. The required name labels the lot; optional criterion explains the reason for grouping, as plain text displayed in the lot list and exports. Example body: {\"name\":\"Milking group\",\"criterion\":\"Animals in lactation\"}. Criterion accepts up to 120 characters, is trimmed and is stored as null when blank or omitted. Animal membership is managed explicitly through PUT /api/v1/ranches/{ranchId}/lots/{lotId}/cattle/{cattleId}. Returns 201 with the created lot; use GET on this collection to list lots.")]
    [ProducesResponseType<LotResource>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateLotAsync(Guid ranchId, CreateLotResource resource,
        CancellationToken cancellationToken)
    {
        var result = await commandService.HandleAsync(new CreateLotCommand(ranchId, resource.Name, resource.Criterion), cancellationToken);
        if (result.IsFailure)
        {
            return CattleInformationActionResultAssembler.ToProblem(this, result.DomainError, localizer);
        }

        return Created($"/api/v1/ranches/{ranchId}/lots", LotResourceFromEntityAssembler.ToResourceFromEntity(result.Value!));
    }

    /// <summary>
    ///     Lists stored ranch lots and memberships, including references to inactive animals.
    /// </summary>
    [HttpGet]
    [EndpointSummary("List lots in an authorized ranch")]
    [EndpointDescription("Returns the complete lot collection of the ranch selected by ranchId in one request, ordered by name then identifier. Requires read access to that ranch. Each lot includes its name, optional grouping reason in criterion, version and stored memberships, including references to inactive animals. Returns an empty array when the ranch has no lots. To create a lot, use POST on this collection.")]
    [ProducesResponseType<IReadOnlyList<LotResource>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetLotsByRanchAsync(Guid ranchId, CancellationToken cancellationToken)
    {
        var result = await queryService.HandleAsync(new GetLotsByRanchQuery(ranchId), cancellationToken);
        if (result.IsFailure)
        {
            return CattleInformationActionResultAssembler.ToProblem(this, result.DomainError, localizer);
        }

        return Ok(result.Value!.Select(LotResourceFromEntityAssembler.ToResourceFromEntity).ToList());
    }

    /// <summary>
    ///     Assigns active cattle from the same ranch using the lot's expected version.
    /// </summary>
    /// <returns>The lot after assignment; a current duplicate membership is unchanged.</returns>
    [HttpPut("{lotId:guid}/cattle/{cattleId:guid}")]
    [EndpointSummary("Assign active cattle to a lot in the same ranch")]
    [ProducesResponseType<LotResource>(StatusCodes.Status200OK)]
    public async Task<IActionResult> AssignCattleToLotAsync(Guid ranchId, Guid lotId, Guid cattleId,
        AssignCattleToLotResource resource, CancellationToken cancellationToken)
    {
        var command = new AssignCattleToLotCommand(ranchId, new LotId(lotId), new CattleId(cattleId), resource.ExpectedVersion);
        var result = await commandService.HandleAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            return CattleInformationActionResultAssembler.ToProblem(this, result.DomainError, localizer);
        }

        return Ok(LotResourceFromEntityAssembler.ToResourceFromEntity(result.Value!));
    }

    /// <summary>
    ///     Removes membership using <paramref name="expectedVersion"/> of the lot while retaining the animal.
    /// </summary>
    /// <returns>The lot after removal, or the unchanged lot when no such membership exists.</returns>
    [HttpDelete("{lotId:guid}/cattle/{cattleId:guid}")]
    [EndpointSummary("Remove cattle from a lot without deleting its identity")]
    [ProducesResponseType<LotResource>(StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveCattleFromLotAsync(Guid ranchId, Guid lotId, Guid cattleId,
        [FromQuery] Guid expectedVersion, CancellationToken cancellationToken)
    {
        var command = new RemoveCattleFromLotCommand(ranchId, new LotId(lotId), new CattleId(cattleId), expectedVersion);
        var result = await commandService.HandleAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            return CattleInformationActionResultAssembler.ToProblem(this, result.DomainError, localizer);
        }

        return Ok(LotResourceFromEntityAssembler.ToResourceFromEntity(result.Value!));
    }
}
