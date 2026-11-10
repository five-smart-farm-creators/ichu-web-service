using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

/// <summary>
///     An unknown operation type becomes null, so that the service reports it together with the other missing fields.
///     A null ranch marks a mobile batch entry, whose ranch is the ranch of its animal.
/// </summary>
public static class RegisterLivestockOperationCommandFromResourceAssembler
{
    public static RegisterLivestockOperationCommand ToCommandFromResource(Guid? ranchId, RegisterLivestockOperationResource resource) =>
        new(
            ranchId,
            resource.OperationId,
            resource.CattleId,
            MonitoringEnumParser.Parse<OperationType>(resource.Type),
            resource.OccurredAt,
            resource.Observation,
            resource.Product,
            resource.Dose);
}
