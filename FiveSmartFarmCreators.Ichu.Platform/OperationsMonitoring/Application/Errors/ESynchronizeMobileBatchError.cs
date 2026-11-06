namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Errors;

/// <summary>
///     Represents the failures that may occur when the mobile application synchronizes a batch of operations.
/// </summary>
public enum ESynchronizeMobileBatchError
{
    /// <summary>No verified account is available for the request.</summary>
    Unauthenticated,

    /// <summary>The batch has no operations or more operations than allowed.</summary>
    InvalidBatchSize
}
