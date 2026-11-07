using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

/// <summary>
///     Assembles the register command of a clinical intervention, which a veterinarian applies, from its REST resource.
/// </summary>
public static class RegisterClinicalInterventionCommandFromResourceAssembler
{
    /// <summary>
    ///     Transforms a <see cref="RegisterLivestockOperationResource"/> into a clinical intervention command.
    /// </summary>
    /// <param name="resource">The HTTP resource containing the clinical intervention data.</param>
    /// <returns>The command to register the clinical intervention.</returns>
    /// <exception cref="ArgumentException">Thrown when the operation type is a field event.</exception>
    public static RegisterLivestockOperationCommand ToCommandFromResource(RegisterLivestockOperationResource resource)
    {
        var command = RegisterLivestockOperationCommandFromResourceAssembler.ToCommandFromResource(resource);
        if (command.Type is { } type && !LivestockOperation.IsClinical(type))
        {
            throw new ArgumentException("Clinical interventions are Vaccination, Treatment or Insemination.");
        }

        return command;
    }
}
