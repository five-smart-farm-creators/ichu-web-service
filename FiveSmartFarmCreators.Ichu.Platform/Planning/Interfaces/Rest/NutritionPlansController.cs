using System.Net.Mime;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Resources;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Transform;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest;

/// <summary>
///     Exposes the nutrition plans of a lot, and the plan in force on a day for accounts that may read the ranch.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/lots/{lotId:guid}/nutrition-plans")]
[Produces(MediaTypeNames.Application.Json)]
public sealed class NutritionPlansController(
    INutritionPlanCommandService commandService,
    INutritionPlanQueryService queryService,
    IStringLocalizer<PlanningMessages> localizer) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("List the nutrition plans of a lot")]
    [EndpointDescription("Returns the nutrition plans of the lot, newest first.")]
    [ProducesResponseType<IReadOnlyList<NutritionPlanResource>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPlansAsync(Guid lotId, CancellationToken cancellationToken)
    {
        var result = await queryService.HandleAsync(new GetNutritionPlansByLotQuery(lotId), cancellationToken);
        if (result.IsFailure)
        {
            return PlanningActionResultAssembler.ToProblem(this, result.DomainError, localizer);
        }

        return Ok(result.Value!.Select(NutritionPlanResourceFromEntityAssembler.ToResourceFromEntity).ToList());
    }

    [HttpGet("current")]
    [EndpointSummary("Get the nutrition plan in force for a lot")]
    [EndpointDescription("Returns the plan that covers the day given by 'on'. Without it, the current UTC date is used, because ranches have no time zone yet.")]
    [ProducesResponseType<NutritionPlanResource>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrentPlanAsync(
        Guid lotId,
        [FromQuery] DateOnly? on,
        CancellationToken cancellationToken)
    {
        var result = await queryService.HandleAsync(new GetCurrentNutritionPlanQuery(lotId, on), cancellationToken);
        if (result.IsFailure)
        {
            return PlanningActionResultAssembler.ToProblem(this, result.DomainError, localizer);
        }

        return Ok(NutritionPlanResourceFromEntityAssembler.ToResourceFromEntity(result.Value!));
    }

    [HttpPost]
    [EndpointSummary("Register a nutrition plan for a lot")]
    [EndpointDescription("Registers the ration of a lot from its start date. A ration change ends the open-ended plan the day before the new one starts, and any other overlap is refused.")]
    [ProducesResponseType<NutritionPlanResource>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterPlanAsync(
        Guid lotId,
        [FromBody] RegisterNutritionPlanResource resource,
        CancellationToken cancellationToken)
    {
        var command = RegisterNutritionPlanCommandFromResourceAssembler.ToCommandFromResource(lotId, resource);
        var result = await commandService.HandleAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            return PlanningActionResultAssembler.ToProblem(this, result.DomainError, localizer);
        }

        return Created($"/api/v1/lots/{lotId}/nutrition-plans",
            NutritionPlanResourceFromEntityAssembler.ToResourceFromEntity(result.Value!));
    }
}
