using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

/// <summary>
///     Assembles the register command of a field event, which an operator observes, from its REST resource.
/// </summary>
public static class RegisterFieldEventCommandFromResourceAssembler
{
    /// <summary>
    ///     Transforms a <see cref="RegisterLivestockOperationResource"/> into a field event command.
    /// </summary>
    /// <param name="resource">The HTTP resource containing the field event data.</param>
    /// <returns>The command to register the field event.</returns>
    /// <exception cref="ArgumentException">Thrown when the operation type is a clinical intervention.</exception>
    public static RegisterLivestockOperationCommand ToCommandFromResource(RegisterLivestockOperationResource resource)
    {
        var command = RegisterLivestockOperationCommandFromResourceAssembler.ToCommandFromResource(resource);
        if (command.Type is { } type && LivestockOperation.IsClinical(type))
        {
            throw new ArgumentException("Field events are HealthEvent or Birth.");
        }

        return command;
    }
}
