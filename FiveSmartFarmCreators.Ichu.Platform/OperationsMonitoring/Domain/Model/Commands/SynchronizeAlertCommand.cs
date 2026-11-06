namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;

/// <summary>
///     Command to synchronize one alert evaluated by an authenticated Edge Gateway.
/// </summary>
/// <param name="GatewayId">The authenticated Edge Gateway.</param>
/// <param name="RanchId">The ranch served by the gateway.</param>
/// <param name="Entry">The alert to synchronize.</param>
/// <param name="ReceivedAt">The instant the cloud received the batch.</param>
public record SynchronizeAlertCommand(Guid GatewayId, Guid RanchId, EdgeAlertEntry Entry, DateTimeOffset ReceivedAt);
