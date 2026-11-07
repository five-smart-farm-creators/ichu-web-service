using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.QueryServices;

/// <summary>
///     Application query service that implements <see cref="IMonitoringRuleSetQueryService"/> to publish the rules of
///     a ranch to its Edge Gateways by revision.
/// </summary>
/// <param name="gatewayAuthenticator">The port that verifies the gateway credentials.</param>
/// <param name="ruleSets">The monitoring rule set repository.</param>
public class MonitoringRuleSetQueryService(
    IEdgeGatewayAuthenticator gatewayAuthenticator,
    IMonitoringRuleSetRepository ruleSets) : IMonitoringRuleSetQueryService
{
    /// <inheritdoc />
    public async Task<Result<MonitoringRuleSnapshot>> Handle(GetEdgeMonitoringRulesQuery query, CancellationToken cancellationToken)
    {
        if (await gatewayAuthenticator.AuthenticateAsync(cancellationToken) is not { } gateway)
        {
            return Result<MonitoringRuleSnapshot>.Failure(OperationsMonitoringErrors.GatewayUnauthenticated, OperationsMonitoringError.GatewayUnauthenticated);
        }

        if (gateway.GatewayId != query.GatewayId)
        {
            return Result<MonitoringRuleSnapshot>.Failure(OperationsMonitoringErrors.GatewayMismatch, OperationsMonitoringError.GatewayMismatch);
        }

        if (query.AfterRevision < 0)
        {
            return Result<MonitoringRuleSnapshot>.Failure(OperationsMonitoringErrors.InvalidRevision, OperationsMonitoringError.InvalidRevision);
        }

        var snapshot = await ruleSets.ReadSnapshotAsync(gateway.RanchId, cancellationToken);
        return snapshot.Revision > query.AfterRevision
            ? Result<MonitoringRuleSnapshot>.Success(snapshot)
            : Result<MonitoringRuleSnapshot>.Failure(OperationsMonitoringErrors.RulesNotModified, OperationsMonitoringError.RulesNotModified);
    }
}
