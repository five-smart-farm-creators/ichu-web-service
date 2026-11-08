namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;

/// <summary>
///     Synchronizes one reading relayed by an authenticated Edge Gateway.
/// </summary>
public record SynchronizeReadingCommand(Guid GatewayId, Guid RanchId, EdgeReadingEntry Entry, DateTimeOffset ReceivedAt);
