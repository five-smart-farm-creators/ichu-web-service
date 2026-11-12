namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

/// <summary>
///     The days open of one calving: from the calving to the latest confirmed conception before the next calving. Both conception and days are null while no conception follows.
/// </summary>
public sealed record ReproductiveInterval(DateTimeOffset CalvingAt, DateTimeOffset? ConceptionServiceAt, int? DaysOpen);
