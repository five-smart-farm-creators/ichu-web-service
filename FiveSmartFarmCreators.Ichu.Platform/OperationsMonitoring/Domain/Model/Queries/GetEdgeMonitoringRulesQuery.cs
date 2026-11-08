namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;

/// <summary>
///     Asks for the monitoring rules that an Edge Gateway does not hold yet.
/// </summary>
public record GetEdgeMonitoringRulesQuery(Guid GatewayId, long AfterRevision);
