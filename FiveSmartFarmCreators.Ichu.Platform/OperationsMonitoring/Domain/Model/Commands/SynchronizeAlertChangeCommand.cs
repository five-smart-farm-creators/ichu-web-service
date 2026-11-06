namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;

/// <summary>
///     Command to synchronize one change to an alert, sent by an authenticated Edge Gateway.
/// </summary>
/// <param name="GatewayId">The authenticated Edge Gateway.</param>
/// <param name="RanchId">The ranch served by the gateway.</param>
/// <param name="Entry">The alert change to synchronize.</param>
/// <param name="ReceivedAt">The instant the cloud received the batch.</param>
public sealed record SynchronizeAlertChangeCommand(Guid GatewayId, Guid RanchId, EdgeAlertChangeEntry Entry, DateTimeOffset ReceivedAt);
