using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

/// <summary>
///     Assembles the register livestock operation command from its REST resource.
/// </summary>
public static class RegisterLivestockOperationCommandFromResourceAssembler
{
    /// <summary>
    ///     Transforms a <see cref="RegisterLivestockOperationResource"/> into a <see cref="RegisterLivestockOperationCommand"/>.
    ///     An unknown operation type becomes <see langword="null"/>, so that the service reports it with the other missing fields.
    /// </summary>
    /// <param name="resource">The HTTP resource containing the operation data.</param>
    /// <returns>The command to register the operation.</returns>
    public static RegisterLivestockOperationCommand ToCommandFromResource(RegisterLivestockOperationResource resource) =>
        new(
            resource.OperationId,
            resource.AnimalId,
            Enum.GetNames<OperationType>().FirstOrDefault(n => n.Equals(resource.Type?.Trim(), StringComparison.OrdinalIgnoreCase)) is { } type
                ? Enum.Parse<OperationType>(type)
                : null,
            resource.OccurredAt,
            resource.Observation,
            resource.Product,
            resource.Dose);
}
