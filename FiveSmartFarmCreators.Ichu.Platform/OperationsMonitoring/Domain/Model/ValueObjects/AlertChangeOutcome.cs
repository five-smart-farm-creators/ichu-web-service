namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

/// <summary>
///     A retry of an applied operation reports AlreadyApplied instead of changing the alert again.
/// </summary>
public enum AlertChangeOutcome { Applied, AlreadyApplied, Conflict }
