namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;

/// <summary>
///     Synchronizes one change to an alert, sent by an authenticated Edge Gateway.
/// </summary>
public record SynchronizeAlertChangeCommand(Guid GatewayId, Guid RanchId, EdgeAlertChangeEntry Entry, DateTimeOffset ReceivedAt);
