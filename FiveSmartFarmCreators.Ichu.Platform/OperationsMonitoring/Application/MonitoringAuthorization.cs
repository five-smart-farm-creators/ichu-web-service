using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Ports;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application;

public sealed class MonitoringAuthorization(IMonitoringAccessControl accessControl, IEdgeGatewayAuthenticator gatewayAuthenticator)
{
    public async Task<Guid> RequireAccountAsync(CancellationToken cancellationToken) =>
        await accessControl.GetCurrentAccountIdAsync(cancellationToken)
        ?? throw MonitoringException.Unauthenticated("A verified account is required.");

    public async Task<Guid> RequireAsync(Guid ranchId, MonitoringPermission permission, CancellationToken cancellationToken)
    {
        var accountId = await RequireAccountAsync(cancellationToken);
        await EnsureAsync(accountId, ranchId, permission, cancellationToken);
        return accountId;
    }

    public async Task EnsureAsync(Guid accountId, Guid ranchId, MonitoringPermission permission, CancellationToken cancellationToken)
    {
        if (!await accessControl.CanAccessRanchAsync(accountId, ranchId, permission, cancellationToken))
        {
            throw MonitoringException.Forbidden("The account is not authorized for this ranch operation.");
        }
    }

    public async Task<EdgeGatewayPrincipal> RequireGatewayAsync(CancellationToken cancellationToken) =>
        await gatewayAuthenticator.AuthenticateAsync(cancellationToken)
        ?? throw MonitoringException.Unauthenticated("Valid Edge Gateway credentials are required.");
}
