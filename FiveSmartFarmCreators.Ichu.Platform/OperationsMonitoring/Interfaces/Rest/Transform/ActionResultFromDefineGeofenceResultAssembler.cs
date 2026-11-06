using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Patterns;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using Microsoft.AspNetCore.Mvc;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

/// <summary>
///     Assembles an HTTP action result from the result of defining a geofence.
/// </summary>
public static class ActionResultFromDefineGeofenceResultAssembler
{
    /// <summary>
    ///     Assembles an <see cref="ActionResult"/> from the result of defining a geofence.
    /// </summary>
    /// <param name="result">The result produced by the application layer.</param>
    /// <param name="controller">The controller used to produce HTTP-specific action results.</param>
    /// <returns>The HTTP action result corresponding to the application outcome.</returns>
    public static ActionResult ToActionResultFromDefineGeofenceResult(
        Result<Geofence, EDefineGeofenceError> result,
        ControllerBase controller) =>
        result switch
        {
            Result<Geofence, EDefineGeofenceError>.Success success =>
                controller.Created($"/api/v1/geofences?ranchId={success.Value.RanchId}",
                    GeofenceResourceFromEntityAssembler.ToResourceFromEntity(success.Value)),
            Result<Geofence, EDefineGeofenceError>.Failure failure => failure.Error switch
            {
                EDefineGeofenceError.Unauthenticated =>
                    controller.Problem(title: "A verified account is required.", statusCode: StatusCodes.Status401Unauthorized),
                EDefineGeofenceError.Forbidden =>
                    controller.Problem(title: "The account is not authorized for this ranch operation.", statusCode: StatusCodes.Status403Forbidden),
                EDefineGeofenceError.LotNotFound =>
                    controller.Problem(title: "The lot does not exist in this ranch.", statusCode: StatusCodes.Status404NotFound),
                EDefineGeofenceError.GeofenceAlreadyDefined =>
                    controller.Problem(title: "The lot already has a geofence; redefine the existing one instead.", statusCode: StatusCodes.Status409Conflict),
                EDefineGeofenceError.ConcurrentModification =>
                    controller.Problem(title: "The rules changed concurrently. Reload them before retrying.", statusCode: StatusCodes.Status409Conflict),
                _ => controller.Problem(statusCode: StatusCodes.Status500InternalServerError)
            },
            _ => controller.Problem(statusCode: StatusCodes.Status500InternalServerError)
        };
}
