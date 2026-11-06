namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.Errors;

/// <summary>
///     Reports a recognized analytics failure using a stable error code.
/// </summary>
public sealed class AnalyticsException(string code, string message) : Exception(message)
{
    /// <summary>
    ///     Gets the code used to map this failure at the application boundary.
    /// </summary>
    public string Code { get; } = code;
}
