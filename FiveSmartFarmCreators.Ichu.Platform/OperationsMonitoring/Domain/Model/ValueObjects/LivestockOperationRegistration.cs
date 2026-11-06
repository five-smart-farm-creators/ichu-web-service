using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

/// <summary>
///     Value object representing the outcome of registering a livestock operation.
/// </summary>
/// <param name="Created">
///     <see langword="false"/> when the same operation was already registered and its stored record is returned.
/// </param>
/// <param name="Operation">The stored livestock operation.</param>
public record LivestockOperationRegistration(bool Created, LivestockOperation Operation);
