using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Model;

/// <summary>
///     Read model of the outcome of registering a livestock operation.
/// </summary>
/// <param name="Created">
///     <see langword="false"/> when the same operation was already registered and its stored record is returned.
/// </param>
/// <param name="Operation">The stored livestock operation.</param>
public record LivestockOperationRegistration(bool Created, LivestockOperation Operation);
