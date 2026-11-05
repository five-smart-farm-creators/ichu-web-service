namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Ports;

/// <summary>
/// Authorization required by Operations and Monitoring. The host implements it with the IAM context; decisions
/// are evaluated for every operation and never trust permissions sent by a client.
/// </summary>
public interface IMonitoringAccessControl
{
    /// <summary>Returns the verified account of the current request, or null when no verified identity exists.</summary>
    Task<Guid?> GetCurrentAccountIdAsync(CancellationToken cancellationToken);

    Task<bool> CanAccessRanchAsync(Guid accountId, Guid ranchId, MonitoringPermission permission, CancellationToken cancellationToken);
}
