using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.Queries;

namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Application.QueryServices;

/// <summary>
///     Defines subscription-controlled executive report exports.
/// </summary>
public interface IExecutiveReportQueryService
{
    /// <summary>
    ///     Exports herd indicators after checking ranch access and subscription entitlement.
    /// </summary>
    /// <param name="query">The ranch, observation window, and requested file format.</param>
    /// <param name="cancellationToken">Cancels source reads and request processing.</param>
    /// <returns>The requested report payload.</returns>
    Task<ExecutiveReport> HandleAsync(GetExecutiveReportQuery query, CancellationToken cancellationToken = default);
}
