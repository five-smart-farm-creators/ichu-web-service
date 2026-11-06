using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Services;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Resources;
using Microsoft.AspNetCore.Mvc;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest;

/// <summary>
/// Exposes explicitly supplied withdrawal period bounds.
/// </summary>
[Route("api/v1/withdrawal-periods")]
public sealed class WithdrawalPeriodsController : PlanningControllerBase
{
    private readonly IPlanningApplicationService _planningService;

    /// <summary>
    /// Initializes a new instance of the <see cref="WithdrawalPeriodsController"/> class.
    /// </summary>
    /// <param name="planningService">Planning application use cases.</param>
    public WithdrawalPeriodsController(IPlanningApplicationService planningService)
    {
        _planningService = planningService;
    }

    /// <summary>
    /// Lists withdrawal periods for a ranch, optionally filtered by animal.
    /// </summary>
    /// <param name="ranchId">The ranch identifier.</param>
    /// <param name="cattleId">An optional animal identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The authorized withdrawal periods.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<WithdrawalPeriodResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public Task<ActionResult<IReadOnlyList<WithdrawalPeriodResource>>> GetWithdrawalPeriods(
        [FromQuery] Guid ranchId,
        [FromQuery] Guid? cattleId,
        CancellationToken cancellationToken)
    {
        return ExecuteAsync<IReadOnlyList<WithdrawalPeriodResource>>(async () =>
        {
            var periods = await _planningService.GetWithdrawalPeriodsAsync(ranchId, cattleId, cancellationToken);
            return periods.Select(PlanningResourceMapper.ToResource).ToArray();
        });
    }

    /// <summary>
    /// Records a withdrawal period using explicit bounds supplied by an authorized caller.
    /// </summary>
    /// <param name="resource">The withdrawal period data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The recorded withdrawal period.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(WithdrawalPeriodResource), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public Task<ActionResult<WithdrawalPeriodResource>> RecordWithdrawalPeriod(
        [FromBody] RecordWithdrawalPeriodResource resource,
        CancellationToken cancellationToken)
    {
        var command = new RecordWithdrawalPeriodCommand(
            resource.RanchId,
            resource.CattleId,
            resource.InterventionId,
            resource.StartsAt,
            resource.EndsAt);

        return ExecuteCreatedAsync(
            async () => PlanningResourceMapper.ToResource(
                await _planningService.RecordWithdrawalPeriodAsync(command, cancellationToken)),
            period => $"/api/v1/withdrawal-periods?ranchId={period.RanchId}&cattleId={period.CattleId}");
    }
}
