using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application;

/// <summary>The registration outcome; the operation is omitted when the identifier belongs to different content.</summary>
public sealed record LivestockOperationResult(SyncEntryStatus Status, LivestockOperation? Operation, string? Reason);
