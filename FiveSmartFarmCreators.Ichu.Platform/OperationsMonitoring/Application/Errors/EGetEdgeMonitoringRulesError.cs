namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Errors;

/// <summary>
///     Represents the outcomes that prevent an Edge Gateway from receiving new monitoring rules.
/// </summary>
public enum EGetEdgeMonitoringRulesError
{
    /// <summary>The Edge Gateway credentials are missing or invalid.</summary>
    Unauthenticated,

    /// <summary>The gateway asked for the configuration of a different gateway.</summary>
    GatewayMismatch,

    /// <summary>The known revision is negative.</summary>
    InvalidRevision,

    /// <summary>The gateway already holds the current revision.</summary>
    NotModified
}
