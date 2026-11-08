namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;

/// <summary>
///     Synchronizes one alert evaluated by an authenticated Edge Gateway.
/// </summary>
public record SynchronizeAlertCommand(Guid GatewayId, Guid RanchId, EdgeAlertEntry Entry, DateTimeOffset ReceivedAt);
