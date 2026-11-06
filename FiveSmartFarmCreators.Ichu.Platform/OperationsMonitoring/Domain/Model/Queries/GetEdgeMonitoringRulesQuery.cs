namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;

/// <summary>
///     Query for the monitoring rules that an Edge Gateway does not hold yet.
/// </summary>
/// <param name="GatewayId">The gateway that asks for its configuration.</param>
/// <param name="AfterRevision">The newest rule revision the gateway already holds.</param>
public sealed record GetEdgeMonitoringRulesQuery(Guid GatewayId, long AfterRevision);
