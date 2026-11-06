using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Errors;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors;

/// <summary>
///     Catalog of the domain errors of the Operations and Monitoring bounded context, one per
///     <see cref="OperationsMonitoringError"/> value.
/// </summary>
public static class OperationsMonitoringErrors
{
    /// <summary>No verified account is available for the request.</summary>
    public static readonly Error Unauthenticated =
        Error.Unauthorized("OperationsMonitoring.Unauthenticated", "A verified account is required.");

    /// <summary>The Edge Gateway credentials are missing or invalid.</summary>
    public static readonly Error GatewayUnauthenticated =
        Error.Unauthorized("OperationsMonitoring.GatewayUnauthenticated", "Valid Edge Gateway credentials are required.");

    /// <summary>The account lacks the required permission on the ranch.</summary>
    public static readonly Error Forbidden =
        Error.Forbidden("OperationsMonitoring.Forbidden", "The account is not authorized for this ranch operation.");

    /// <summary>The gateway asked for the configuration of a different gateway.</summary>
    public static readonly Error GatewayMismatch =
        Error.Forbidden("OperationsMonitoring.GatewayMismatch", "A gateway can only read its own configuration.");

    /// <summary>The alert does not exist.</summary>
    public static readonly Error AlertNotFound =
        Error.NotFound("OperationsMonitoring.AlertNotFound", "The alert does not exist.");

    /// <summary>The animal does not exist.</summary>
    public static readonly Error AnimalNotFound =
        Error.NotFound("OperationsMonitoring.AnimalNotFound", "The animal does not exist.");

    /// <summary>The lot does not exist in the ranch.</summary>
    public static readonly Error LotNotFound =
        Error.NotFound("OperationsMonitoring.LotNotFound", "The lot does not exist in this ranch.");

    /// <summary>The geofence does not exist.</summary>
    public static readonly Error GeofenceNotFound =
        Error.NotFound("OperationsMonitoring.GeofenceNotFound", "The geofence does not exist.");

    /// <summary>The lot already has a geofence.</summary>
    public static readonly Error GeofenceAlreadyDefined =
        Error.Conflict("OperationsMonitoring.GeofenceAlreadyDefined", "The lot already has a geofence; redefine the existing one instead.");

    /// <summary>The alert version is stale, or a different operation already attended the alert.</summary>
    public static readonly Error AlertConflict =
        Error.Conflict("OperationsMonitoring.AlertConflict", "The alert was already attended or changed by a different operation.");

    /// <summary>A different operation already uses the identifier.</summary>
    public static readonly Error OperationConflict =
        Error.Conflict("OperationsMonitoring.OperationConflict", "A different operation already uses this identifier.");

    /// <summary>The animal was not active in the herd when the operation occurred.</summary>
    public static readonly Error AnimalNotActive =
        Error.Conflict("OperationsMonitoring.AnimalNotActive", "The animal was not active in the herd when the operation occurred.");

    /// <summary>The rules changed concurrently; reload them before retrying.</summary>
    public static readonly Error ConcurrentModification =
        Error.Conflict("OperationsMonitoring.ConcurrentModification", "The rules changed concurrently. Reload them before retrying.");

    /// <summary>The event time is ahead of the server clock beyond the configured tolerance.</summary>
    public static readonly Error EventTimeAhead =
        Error.Validation("OperationsMonitoring.EventTimeAhead", "The event time is ahead of the server clock beyond the configured tolerance.");

    /// <summary>The page is not positive or the page size is outside the allowed range.</summary>
    public static readonly Error InvalidPage =
        Error.Validation("OperationsMonitoring.InvalidPage", "The page must be positive and the page size within the allowed range.");

    /// <summary>The known rule revision is negative.</summary>
    public static readonly Error InvalidRevision =
        Error.Validation("OperationsMonitoring.InvalidRevision", "The known revision cannot be negative.");

    /// <summary>The Edge Gateway batch has no entries or more entries than allowed.</summary>
    public static readonly Error InvalidEdgeBatchSize =
        Error.Validation("OperationsMonitoring.InvalidEdgeBatchSize", "A batch must contain at least one entry and no more than the configured maximum.");

    /// <summary>The mobile batch has no operations or more operations than allowed.</summary>
    public static readonly Error InvalidMobileBatchSize =
        Error.Validation("OperationsMonitoring.InvalidMobileBatchSize", "A batch must contain at least one operation and no more than the configured maximum.");

    /// <summary>The gateway already holds the current rule revision; it is reported without a response body.</summary>
    public static readonly Error RulesNotModified =
        Error.Failure("OperationsMonitoring.RulesNotModified", "The gateway already holds the current revision.");
}
