using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Services;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.CommandServices;

/// <summary>
///     Defines and redefines the geofences of a ranch. Each change advances the rule revision in the same unit of work.
/// </summary>
public class GeofenceCommandService(
    IMonitoringAccessControl accessControl,
    ICattleLookup cattleLookup,
    IGeofenceRepository geofences,
    IMonitoringRuleRevisionService ruleRevisionService,
    IOperationsMonitoringUnitOfWork unitOfWork) : IGeofenceCommandService
{
    /// <inheritdoc />
    public async Task<Result<Geofence>> Handle(DefineGeofenceCommand command, CancellationToken cancellationToken)
    {
        if (await accessControl.GetCurrentAccountIdAsync(cancellationToken) is not { } accountId)
        {
            return Result<Geofence>.Failure(OperationsMonitoringErrors.Unauthenticated);
        }

        if (!await accessControl.CanAccessRanchAsync(accountId, command.RanchId, MonitoringPermission.ManageRules, cancellationToken))
        {
            return Result<Geofence>.Failure(OperationsMonitoringErrors.Forbidden);
        }

        if (!await cattleLookup.LotBelongsToRanchAsync(command.LotId, command.RanchId, cancellationToken))
        {
            return Result<Geofence>.Failure(OperationsMonitoringErrors.LotNotFound);
        }

        if (await geofences.FindByLotAsync(command.RanchId, command.LotId, cancellationToken) is not null)
        {
            return Result<Geofence>.Failure(OperationsMonitoringErrors.GeofenceAlreadyDefined);
        }

        var revision = await ruleRevisionService.AdvanceAsync(command.RanchId, cancellationToken);
        var creation = Geofence.Create(command.RanchId, command.LotId, command.Name, command.ToleranceMeters,
            command.Severity, command.Boundary, revision);
        if (creation.IsFailure)
        {
            return creation;
        }

        await geofences.AddAsync(creation.Value!, cancellationToken);
        return await unitOfWork.TryCompleteAsync(cancellationToken)
            ? creation
            : Result<Geofence>.Failure(OperationsMonitoringErrors.ConcurrentModification);
    }

    /// <inheritdoc />
    public async Task<Result<Geofence>> Handle(RedefineGeofenceCommand command, CancellationToken cancellationToken)
    {
        if (await accessControl.GetCurrentAccountIdAsync(cancellationToken) is not { } accountId)
        {
            return Result<Geofence>.Failure(OperationsMonitoringErrors.Unauthenticated);
        }

        if (!await accessControl.CanAccessRanchAsync(accountId, command.RanchId, MonitoringPermission.ManageRules, cancellationToken))
        {
            return Result<Geofence>.Failure(OperationsMonitoringErrors.Forbidden);
        }

        // The ranch is checked before the lookup, and a geofence of another ranch is reported as missing, so identifiers cannot be probed.
        var geofence = await geofences.FindByIdAsync(command.GeofenceId, cancellationToken);
        if (geofence is null || geofence.RanchId != command.RanchId)
        {
            return Result<Geofence>.Failure(OperationsMonitoringErrors.GeofenceNotFound);
        }

        var revision = await ruleRevisionService.AdvanceAsync(geofence.RanchId, cancellationToken);
        var redefinition = geofence.Redefine(command.Name, command.ToleranceMeters, command.Severity, command.Boundary, revision);
        if (redefinition.IsFailure)
        {
            return Result<Geofence>.Failure(redefinition.DomainError);
        }

        return await unitOfWork.TryCompleteAsync(cancellationToken)
            ? Result<Geofence>.Success(geofence)
            : Result<Geofence>.Failure(OperationsMonitoringErrors.ConcurrentModification);
    }
}
