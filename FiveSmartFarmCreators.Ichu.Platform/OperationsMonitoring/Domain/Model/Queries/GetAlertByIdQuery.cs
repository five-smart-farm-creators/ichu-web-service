namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;

/// <summary>
///     Query to retrieve an alert by its identifier.
/// </summary>
/// <param name="AlertId">The alert identifier.</param>
public record GetAlertByIdQuery(Guid AlertId);
