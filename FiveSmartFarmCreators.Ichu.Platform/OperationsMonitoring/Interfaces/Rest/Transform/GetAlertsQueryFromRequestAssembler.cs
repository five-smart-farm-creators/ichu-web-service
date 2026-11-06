using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

/// <summary>
///     Assembles the alert listing query from the parameters of the request.
/// </summary>
public static class GetAlertsQueryFromRequestAssembler
{
    /// <summary>
    ///     Builds a <see cref="GetAlertsQuery"/> from the query string values.
    /// </summary>
    /// <param name="ranchId">The ranch that owns the alerts.</param>
    /// <param name="status">The optional status name: Pending or Acknowledged.</param>
    /// <param name="animalId">The optional animal filter.</param>
    /// <param name="page">The one-based page number.</param>
    /// <param name="pageSize">The maximum number of alerts per page.</param>
    /// <returns>The query to list the alerts.</returns>
    /// <exception cref="ArgumentException">Thrown when the status is not a known alert status.</exception>
    public static GetAlertsQuery ToQueryFromRequest(Guid ranchId, string? status, Guid? animalId, int page, int pageSize)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return new GetAlertsQuery(ranchId, null, animalId, page, pageSize);
        }

        var name = Enum.GetNames<AlertStatus>().FirstOrDefault(n => n.Equals(status.Trim(), StringComparison.OrdinalIgnoreCase));
        if (name is null)
        {
            throw new ArgumentException($"The status must be one of: {string.Join(", ", Enum.GetNames<AlertStatus>())}.");
        }

        return new GetAlertsQuery(ranchId, Enum.Parse<AlertStatus>(name), animalId, page, pageSize);
    }
}
