using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Model;

/// <summary>
///     Created is false when the same operation was already registered, and the stored record is returned instead.
/// </summary>
public record LivestockOperationRegistration(bool Created, LivestockOperation Operation);
