using System.Globalization;
using System.Text;
using System.Text.Json;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Application.Internal.QueryServices;

/// <summary>
///     Generates executive reports after the format, access, entitlement and data checks, in that order, so a refused export never reaches the file.
/// </summary>
public sealed class ExecutiveReportQueryService(IAnalyticsQueryService analytics, IAnalyticsEntitlements entitlements, TimeProvider clock)
    : IExecutiveReportQueryService
{
    /// <inheritdoc />
    public async Task<Result<ExecutiveReport>> HandleAsync(GetExecutiveReportQuery query, CancellationToken cancellationToken = default)
    {
        var (ranchId, period, format) = query;
        if (format is not ("json" or "csv"))
        {
            return Result<ExecutiveReport>.Failure(DashboardAnalyticsErrors.UnsupportedReportFormat);
        }

        var result = await analytics.HandleAsync(new GetHerdIndicatorsQuery(ranchId, period), cancellationToken);
        if (result.IsFailure)
        {
            return Result<ExecutiveReport>.Failure(result.DomainError);
        }

        var indicators = result.Value!;
        if (!await entitlements.CanExportAsync(ranchId, cancellationToken))
        {
            return Result<ExecutiveReport>.Failure(DashboardAnalyticsErrors.ExportNotIncluded);
        }

        if (indicators.MonitoredAnimals.Value is null or 0 && indicators.MortalityRate.Denominator is null or 0)
        {
            return Result<ExecutiveReport>.Failure(DashboardAnalyticsErrors.NoDataToExport);
        }

        var generatedAt = clock.GetUtcNow();
        var fileName = $"executive-report-{ranchId}-{generatedAt:yyyyMMddHHmmss}.{format}";
        if (format == "json")
        {
            return Result<ExecutiveReport>.Success(new ExecutiveReport(fileName, "application/json", JsonSerializer.SerializeToUtf8Bytes(
                new { GeneratedAt = generatedAt, Indicators = indicators }, new JsonSerializerOptions(JsonSerializerDefaults.Web))));
        }

        var rows = new List<string>
        {
            "Metric,Value,Unit,Formula,Denominator,UnavailableReason,From,To,UpdatedAt,GeneratedAt,Source"
        };
        foreach (var (name, value) in new[] { ("MonitoredAnimals", indicators.MonitoredAnimals),
            ("OutsideGeofenceAnimals", indicators.OutsideGeofenceAnimals), ("MortalityRate", indicators.MortalityRate), ("Births", indicators.Births), ("ConfirmedPregnancies", indicators.ConfirmedPregnancies) })
        {
            rows.Add(string.Join(',', new[] { name, value.Value?.ToString(CultureInfo.InvariantCulture), value.Unit,
                value.Formula, value.Denominator?.ToString(CultureInfo.InvariantCulture), value.UnavailableReason,
                period.From.ToString("O"), period.To.ToString("O"), indicators.UpdatedAt.ToString("O"), generatedAt.ToString("O"), indicators.Source }.Select(Escape)));
        }

        return Result<ExecutiveReport>.Success(new ExecutiveReport(fileName, "text/csv; charset=utf-8", Encoding.UTF8.GetBytes(string.Join("\r\n", rows))));
    }

    private static string Escape(string? value)
    {
        var text = value ?? string.Empty;
        // Neutralize spreadsheet formulas before quoting CSV fields.
        if (text.TrimStart().StartsWith('=') || text.TrimStart().StartsWith('+') ||
            text.TrimStart().StartsWith('-') || text.TrimStart().StartsWith('@'))
        {
            text = "'" + text;
        }

        return "\"" + text.Replace("\"", "\"\"") + "\"";
    }
}
