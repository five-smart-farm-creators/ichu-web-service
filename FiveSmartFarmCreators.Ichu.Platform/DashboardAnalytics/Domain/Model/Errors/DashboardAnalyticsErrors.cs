using System.Globalization;
using System.Resources;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Resources;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Errors;

namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.Errors;

/// <summary>
///     Business errors of the DashboardAnalytics context. Each code is also the resource key of its localized message, so the English text lives in one place.
/// </summary>
public static class DashboardAnalyticsErrors
{
    // Declared first because the error fields below read their messages during static initialization.
    private static readonly ResourceManager Messages = new(typeof(DashboardAnalyticsMessages));

    public static readonly Error InvalidPeriod = Define("InvalidPeriod", EErrorType.Validation);
    public static readonly Error RanchRequired = Define("RanchRequired", EErrorType.Validation);
    public static readonly Error LotRequired = Define("LotRequired", EErrorType.Validation);
    public static readonly Error InvalidOutlierDeviation = Define("InvalidOutlierDeviation", EErrorType.Validation);
    public static readonly Error UnsupportedMetric = Define("UnsupportedMetric", EErrorType.Validation);
    public static readonly Error ConditionRequired = Define("ConditionRequired", EErrorType.Validation);
    public static readonly Error UnsupportedReportFormat = Define("UnsupportedReportFormat", EErrorType.Validation);

    public static readonly Error RanchAccessDenied = Define("RanchAccessDenied", EErrorType.Forbidden);
    public static readonly Error ExportNotIncluded = Define("ExportNotIncluded", EErrorType.Forbidden);

    public static readonly Error NoDataToExport = Define("NoDataToExport", EErrorType.Unprocessable);
    public static readonly Error SourceUnavailable = Define("SourceUnavailable", EErrorType.Unavailable);

    private static Error Define(string name, EErrorType type)
    {
        var code = $"DashboardAnalytics.{name}";
        return new Error(code, Messages.GetString(code, CultureInfo.InvariantCulture) ?? code, type);
    }
}
