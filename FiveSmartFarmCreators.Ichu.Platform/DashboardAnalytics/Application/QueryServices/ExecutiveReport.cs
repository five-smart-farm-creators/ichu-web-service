namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Application.QueryServices;

/// <summary>
///     Carries an exported report filename, media type, and generated file bytes.
/// </summary>
public sealed record ExecutiveReport(string FileName, string ContentType, byte[] Content);
