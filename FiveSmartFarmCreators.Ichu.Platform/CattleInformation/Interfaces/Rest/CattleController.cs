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
///     Exposes ranch-authorized animal registration, profiles and lifecycle changes.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/ranches/{ranchId:guid}/cattle")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Cattle Information")]
public sealed class CattleController(
    ICattleCommandService commandService,
    ICattleQueryService queryService,
    IStringLocalizer<CattleInformationMessages> localizer) : ControllerBase
{
    /// <summary>
    ///     Registers an active animal with an ear tag that is unique within the authorized ranch.
    /// </summary>
    /// <returns>The new animal and the URI of its profile.</returns>
    [HttpPost]
    [EndpointSummary("Register cattle in a ranch")]
    [ProducesResponseType<CattleResource>(StatusCodes.Status201Created)]
    public async Task<IActionResult> RegisterCattleAsync(Guid ranchId, RegisterCattleResource resource,
        CancellationToken cancellationToken)
    {
        var command = new RegisterCattleCommand(
            ranchId, resource.EarTag, resource.Breed, resource.Sex, resource.BirthDate, resource.LifeStage);
        var result = await commandService.HandleAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            return CattleInformationActionResultAssembler.ToProblem(this, result.DomainError, localizer);
        }

        var cattle = result.Value!;
        return CreatedAtAction(nameof(GetCattleByIdAsync), new { ranchId, cattleId = cattle.Id.Value },
            CattleResourceFromEntityAssembler.ToResourceFromEntity(cattle));
    }

    /// <summary>
    ///     Combines an authorized animal's retained details with explicit device-integration availability.
    /// </summary>
    /// <returns>The profile, including a distinction between unavailable device data and no collar assignment.</returns>
    [HttpGet("{cattleId:guid}")]
    [ActionName(nameof(GetCattleByIdAsync))]
    [EndpointSummary("Get the authorized cattle profile")]
    [ProducesResponseType<CattleProfileResource>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCattleByIdAsync(Guid ranchId, Guid cattleId,
        CancellationToken cancellationToken)
    {
        var result = await queryService.HandleAsync(new GetCattleByIdQuery(ranchId, new CattleId(cattleId)), cancellationToken);
        if (result.IsFailure)
        {
            return CattleInformationActionResultAssembler.ToProblem(this, result.DomainError, localizer);
        }

        return Ok(CattleProfileResourceFromModelAssembler.ToResourceFromModel(result.Value!));
    }

    /// <summary>
    ///     Searches one authorized ranch, retaining inactive animals unless a status filter is supplied.
    /// </summary>
    /// <returns>A page ordered by ear tag and identifier, or an empty array when none match.</returns>
    [HttpGet]
    [EndpointSummary("Search cattle in an authorized ranch")]
    [ProducesResponseType<IReadOnlyList<CattleResource>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchCattleAsync(Guid ranchId, CancellationToken cancellationToken,
        [FromQuery] string? earTag = null, [FromQuery] CattleStatus? status = null,
        [FromQuery] Guid? lotId = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var query = new SearchCattleQuery(ranchId, earTag, status, lotId is { } id ? new LotId(id) : null, page, pageSize);
        var result = await queryService.HandleAsync(query, cancellationToken);
        if (result.IsFailure)
        {
            return CattleInformationActionResultAssembler.ToProblem(this, result.DomainError, localizer);
        }

        return Ok(result.Value!.Select(CattleResourceFromEntityAssembler.ToResourceFromEntity).ToList());
    }

    /// <summary>
    ///     Corrects active animal details using the expected version while preserving stage history.
    /// </summary>
    [HttpPut("{cattleId:guid}")]
    [Obsolete("Use PATCH /api/v1/cattle/{cattleId} with the fields to change and expectedVersion.")]
    [EndpointSummary("Update cattle identification and productive details")]
    [EndpointDescription("Replaces earTag, breed, sex and birthDate using expectedVersion. For partial updates and combined productive-stage changes, use PATCH /api/v1/cattle/{cattleId}.")]
    [ProducesResponseType<CattleResource>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateCattleAsync(Guid ranchId, Guid cattleId, UpdateCattleResource resource,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCattleCommand(
            ranchId, new CattleId(cattleId), resource.EarTag, resource.Breed, resource.Sex, resource.BirthDate,
            resource.ExpectedVersion);
        var result = await commandService.HandleAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            return CattleInformationActionResultAssembler.ToProblem(this, result.DomainError, localizer);
        }

        return Ok(CattleResourceFromEntityAssembler.ToResourceFromEntity(result.Value!));
    }

    [HttpPatch("~/api/v1/cattle/{cattleId:guid}")]
    [EndpointSummary("Partially update cattle by its unique identifier")]
    [EndpointDescription("The server resolves the animal's ranch from its globally unique cattleId and requires permission to manage that ranch. Supply expectedVersion and any of earTag, breed, sex, birthDate or lifeStage. Omitted or null editable fields retain their values; unknown fields are rejected. A lifeStage change requires validFrom, retains the preceding period and opens a new one. Productive stages include Calf (young cattle), Heifer, MilkingCow (milk production) and FatteningBull. The effective date must fall within the current period, on or before today and on or after birth. All supplied changes are validated and saved together; stale versions return 409. Collar assignment is managed through POST /api/v1/ranches/{ranchId}/devices/{deviceId}/assignments.")]
    [ProducesResponseType<CattleResource>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> PatchCattleAsync(Guid cattleId, PatchCattleResource resource,
        CancellationToken cancellationToken)
    {
        var command = new PatchCattleCommand(new CattleId(cattleId), resource.ExpectedVersion!.Value,
            resource.EarTag, resource.Breed, resource.Sex, resource.BirthDate, resource.LifeStage, resource.ValidFrom);
        var result = await commandService.HandleAsync(command, cancellationToken);
        return result.IsFailure
            ? CattleInformationActionResultAssembler.ToProblem(this, result.DomainError, localizer)
            : Ok(CattleResourceFromEntityAssembler.ToResourceFromEntity(result.Value!));
    }

    /// <summary>
    ///     Records an effective-dated stage change while retaining the preceding period, including same-day changes.
    /// </summary>
    [HttpPatch("{cattleId:guid}/life-stage")]
    [Obsolete("Use PATCH /api/v1/cattle/{cattleId} with lifeStage, validFrom and expectedVersion.")]
    [EndpointSummary("Change the productive stage while retaining its history")]
    [EndpointDescription("Records a productive stage such as Calf, Heifer, MilkingCow or FatteningBull with validFrom and expectedVersion. To combine stage and identity changes atomically, use PATCH /api/v1/cattle/{cattleId}. Collars are managed through /api/v1/ranches/{ranchId}/devices/{deviceId}/assignments.")]
    [ProducesResponseType<CattleResource>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ChangeLifeStageAsync(Guid ranchId, Guid cattleId, ChangeLifeStageResource resource,
        CancellationToken cancellationToken)
    {
        var command = new ChangeLifeStageCommand(
            ranchId, new CattleId(cattleId), resource.LifeStage, resource.ValidFrom, resource.ExpectedVersion);
        var result = await commandService.HandleAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            return CattleInformationActionResultAssembler.ToProblem(this, result.DomainError, localizer);
        }

        return Ok(CattleResourceFromEntityAssembler.ToResourceFromEntity(result.Value!));
    }

    /// <summary>
    ///     Records an animal's exit and keeps its history; the collar release happens after the exit is saved.
    /// </summary>
    /// <returns>The inactive animal; the response does not wait for the collar release.</returns>
    [HttpPost("{cattleId:guid}/deactivations")]
    [EndpointSummary("Deactivate cattle and retain the exit reason")]
    [ProducesResponseType<CattleResource>(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeactivateCattleAsync(Guid ranchId, Guid cattleId, DeactivateCattleResource resource,
        CancellationToken cancellationToken)
    {
        var command = new DeactivateCattleCommand(
            ranchId, new CattleId(cattleId), resource.Reason, resource.ExitDate, resource.ExpectedVersion);
        var result = await commandService.HandleAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            return CattleInformationActionResultAssembler.ToProblem(this, result.DomainError, localizer);
        }

        return Ok(CattleResourceFromEntityAssembler.ToResourceFromEntity(result.Value!));
    }
}
