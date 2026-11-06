namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Errors;

/// <summary>
///     Represents the failures that may occur when an Edge Gateway synchronizes a batch.
/// </summary>
public enum ESynchronizeEdgeBatchError
{
    /// <summary>The Edge Gateway credentials are missing or invalid.</summary>
    Unauthenticated,

    /// <summary>The batch has no entries or more entries than allowed.</summary>
    InvalidBatchSize
}
