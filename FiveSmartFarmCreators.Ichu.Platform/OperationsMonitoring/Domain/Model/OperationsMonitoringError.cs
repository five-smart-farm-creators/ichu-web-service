namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model;

/// <summary>
///     Represents the failures that may occur in the Operations and Monitoring bounded context.
/// </summary>
public enum OperationsMonitoringError
{
    /// <summary>No verified account is available for the request.</summary>
    Unauthenticated,

    /// <summary>The Edge Gateway credentials are missing or invalid.</summary>
    GatewayUnauthenticated,

    /// <summary>The account lacks the required permission on the ranch.</summary>
    Forbidden,

    /// <summary>The gateway asked for the configuration of a different gateway.</summary>
    GatewayMismatch,

    /// <summary>The alert does not exist.</summary>
    AlertNotFound,

    /// <summary>The animal does not exist.</summary>
    AnimalNotFound,

    /// <summary>The lot does not exist in the ranch.</summary>
    LotNotFound,

    /// <summary>The geofence does not exist.</summary>
    GeofenceNotFound,

    /// <summary>The lot already has a geofence.</summary>
    GeofenceAlreadyDefined,

    /// <summary>The alert version is stale, or a different operation already attended the alert.</summary>
    AlertConflict,

    /// <summary>A different operation already uses the identifier.</summary>
    OperationConflict,

    /// <summary>The animal was not active in the herd when the operation occurred.</summary>
    AnimalNotActive,

    /// <summary>The rules changed concurrently; reload them before retrying.</summary>
    ConcurrentModification,

    /// <summary>The event time is ahead of the server clock beyond the configured tolerance.</summary>
    EventTimeAhead,

    /// <summary>The page is not positive or the page size is outside the allowed range.</summary>
    InvalidPage,

    /// <summary>The known rule revision is negative.</summary>
    InvalidRevision,

    /// <summary>The Edge Gateway batch has no entries or more entries than allowed.</summary>
    InvalidEdgeBatchSize,

    /// <summary>The mobile batch has no operations or more operations than allowed.</summary>
    InvalidMobileBatchSize,

    /// <summary>The gateway already holds the current rule revision.</summary>
    RulesNotModified
}
