using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;

/// <summary>
///     Retrieves one alert by its identifier.
/// </summary>
public record GetAlertByIdQuery(MonitoringAlertId AlertId);
