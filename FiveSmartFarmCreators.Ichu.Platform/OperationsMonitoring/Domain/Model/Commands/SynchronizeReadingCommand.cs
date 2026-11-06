namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;

/// <summary>
///     Command to synchronize one reading relayed by an authenticated Edge Gateway.
/// </summary>
/// <param name="GatewayId">The authenticated Edge Gateway.</param>
/// <param name="RanchId">The ranch served by the gateway.</param>
/// <param name="Entry">The reading to synchronize.</param>
/// <param name="ReceivedAt">The instant the cloud received the batch.</param>
public sealed record SynchronizeReadingCommand(Guid GatewayId, Guid RanchId, EdgeReadingEntry Entry, DateTimeOffset ReceivedAt);
