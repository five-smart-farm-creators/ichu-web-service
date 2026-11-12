namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

/// <summary>
///     The open period after a calving. Conception and days are null while no conception follows the calving.
/// </summary>
public record ReproductiveIntervalResource(DateTimeOffset CalvingAt, DateTimeOffset? ConceptionServiceAt, int? DaysOpen);
