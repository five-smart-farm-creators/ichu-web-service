using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices.ACL;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Patterns;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Services;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;

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
    public async Task<Result<MonitoringRuleSnapshot, EGetEdgeMonitoringRulesError>> Handle(GetEdgeMonitoringRulesQuery query, CancellationToken cancellationToken)
    {
        if (await gatewayAuthenticator.AuthenticateAsync(cancellationToken) is not { } gateway)
        {
            return new Result<MonitoringRuleSnapshot, EGetEdgeMonitoringRulesError>.Failure(EGetEdgeMonitoringRulesError.Unauthenticated);
        }

        if (gateway.GatewayId != query.GatewayId)
        {
            return new Result<MonitoringRuleSnapshot, EGetEdgeMonitoringRulesError>.Failure(EGetEdgeMonitoringRulesError.GatewayMismatch);
        }

        if (query.AfterRevision < 0)
        {
            return new Result<MonitoringRuleSnapshot, EGetEdgeMonitoringRulesError>.Failure(EGetEdgeMonitoringRulesError.InvalidRevision);
        }

        var snapshot = await ruleSets.ReadSnapshotAsync(gateway.RanchId, cancellationToken);
        return snapshot.Revision > query.AfterRevision
            ? new Result<MonitoringRuleSnapshot, EGetEdgeMonitoringRulesError>.Success(snapshot)
            : new Result<MonitoringRuleSnapshot, EGetEdgeMonitoringRulesError>.Failure(EGetEdgeMonitoringRulesError.NotModified);
    }
}
