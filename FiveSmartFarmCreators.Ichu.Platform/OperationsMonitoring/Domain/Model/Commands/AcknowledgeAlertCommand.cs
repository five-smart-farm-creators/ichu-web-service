using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;

/// <summary>
///     Attention for a pending alert. The operation identifier is reused on every retry of the same request.
/// </summary>
public record AcknowledgeAlertCommand(Guid RanchId, MonitoringAlertId AlertId, Guid OperationId, string Finding, string? Action, int ExpectedVersion);
