namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;

public sealed record AnimalSnapshot(Guid AnimalId, Guid LotId, bool DiedInPeriod, bool OutsideGeofence, int? ExpectedReadings = null);
