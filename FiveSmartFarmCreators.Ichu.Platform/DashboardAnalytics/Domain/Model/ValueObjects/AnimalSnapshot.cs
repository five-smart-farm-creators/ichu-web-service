namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;

/// <summary>
///     Describes lot membership and observed animal state within an analysis window.
/// </summary>
public sealed record AnimalSnapshot(Guid AnimalId, Guid LotId, bool DiedInPeriod, bool OutsideGeofence, int? ExpectedReadings = null);
