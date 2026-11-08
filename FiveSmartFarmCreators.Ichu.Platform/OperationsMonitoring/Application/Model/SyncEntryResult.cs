using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Model;

/// <summary>
///     The reason is English text for the gateway and is only present when the outcome needs an explanation.
/// </summary>
public record SyncEntryResult(
    SyncEntryKind Kind,
    Guid Id,
    SyncEntryStatus Status,
    string? Reason,
    MonitoringAlertId? RelatedAlertId = null);
