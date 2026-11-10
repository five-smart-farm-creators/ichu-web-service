using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

public static class RegisterClinicalInterventionCommandFromResourceAssembler
{
    public static Result<RegisterLivestockOperationCommand> ToCommandFromResource(Guid ranchId, RegisterLivestockOperationResource resource)
    {
        var command = RegisterLivestockOperationCommandFromResourceAssembler.ToCommandFromResource(ranchId, resource);
        return command.Type is { } type && !LivestockOperation.IsClinical(type)
            ? Result<RegisterLivestockOperationCommand>.Failure(OperationsMonitoringErrors.ClinicalInterventionTypeOnly)
            : Result<RegisterLivestockOperationCommand>.Success(command);
    }
}
