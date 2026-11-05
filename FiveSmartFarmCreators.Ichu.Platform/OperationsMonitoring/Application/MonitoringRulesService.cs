using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Ports;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application;

/// <summary>Manages thresholds and geofences (US-15) and publishes them to Edge Gateways by revision (TS-15).</summary>
public sealed class MonitoringRulesService(
    MonitoringAuthorization authorization,
    ICattleLookup cattleLookup,
    IMonitoringThresholdRepository thresholds,
    IGeofenceRepository geofences,
    IMonitoringRuleSetRepository ruleSets,
    IOperationsMonitoringUnitOfWork unitOfWork)
{
    public async Task<MonitoringThreshold> ConfigureThresholdAsync(ConfigureThresholdCommand command, CancellationToken cancellationToken)
    {
        await authorization.RequireAsync(command.RanchId, MonitoringPermission.ManageRules, cancellationToken);
        var lifeStage = MonitoringThreshold.NormalizeLifeStage(command.LifeStage);
        var revision = await AdvanceRevisionAsync(command.RanchId, cancellationToken);

        var threshold = await thresholds.FindAsync(command.RanchId, lifeStage, command.Metric, cancellationToken);
        if (threshold is null)
        {
            threshold = new MonitoringThreshold(command.RanchId, lifeStage, command.Metric, command.Range, command.Severity, revision);
            thresholds.Add(threshold);
        }
        else
        {
            threshold.Reconfigure(command.Range, command.Severity, revision);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return threshold;
    }

    public async Task<IReadOnlyList<MonitoringThreshold>> GetThresholdsAsync(Guid ranchId, CancellationToken cancellationToken)
    {
        await authorization.RequireAsync(ranchId, MonitoringPermission.ReadMonitoring, cancellationToken);
        return await thresholds.FindByRanchAsync(ranchId, cancellationToken);
    }

    public async Task<Geofence> DefineGeofenceAsync(DefineGeofenceCommand command, CancellationToken cancellationToken)
    {
        await authorization.RequireAsync(command.RanchId, MonitoringPermission.ManageRules, cancellationToken);
        if (!await cattleLookup.LotBelongsToRanchAsync(command.LotId, command.RanchId, cancellationToken))
        {
            throw MonitoringException.NotFound("The lot does not exist in this ranch.");
        }

        if (await geofences.FindByLotAsync(command.RanchId, command.LotId, cancellationToken) is not null)
        {
            throw MonitoringException.Conflict("The lot already has a geofence; redefine the existing one instead.");
        }

        var revision = await AdvanceRevisionAsync(command.RanchId, cancellationToken);
        var geofence = new Geofence(command.RanchId, command.LotId, command.Name, command.ToleranceMeters,
            command.Severity, command.Boundary, revision);
        geofences.Add(geofence);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return geofence;
    }

    public async Task<Geofence> RedefineGeofenceAsync(RedefineGeofenceCommand command, CancellationToken cancellationToken)
    {
        var accountId = await authorization.RequireAccountAsync(cancellationToken);
        var geofence = await geofences.FindAsync(command.GeofenceId, cancellationToken)
            ?? throw MonitoringException.NotFound("The geofence does not exist.");
        await authorization.EnsureAsync(accountId, geofence.RanchId, MonitoringPermission.ManageRules, cancellationToken);

        var revision = await AdvanceRevisionAsync(geofence.RanchId, cancellationToken);
        geofence.Redefine(command.Name, command.ToleranceMeters, command.Severity, command.Boundary, revision);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return geofence;
    }

    public async Task<IReadOnlyList<Geofence>> GetGeofencesAsync(Guid ranchId, CancellationToken cancellationToken)
    {
        await authorization.RequireAsync(ranchId, MonitoringPermission.ReadMonitoring, cancellationToken);
        return await geofences.FindByRanchAsync(ranchId, cancellationToken);
    }

    /// <summary>Returns the rules of the gateway's ranch, or null when the gateway already holds the current revision.</summary>
    public async Task<MonitoringRuleSnapshot?> GetEdgeRulesAsync(Guid gatewayId, long afterRevision, CancellationToken cancellationToken)
    {
        var gateway = await authorization.RequireGatewayAsync(cancellationToken);
        if (gateway.GatewayId != gatewayId)
        {
            throw MonitoringException.Forbidden("A gateway can only read its own configuration.");
        }

        if (afterRevision < 0)
        {
            throw MonitoringException.InvalidInput("The known revision cannot be negative.");
        }

        var snapshot = await ruleSets.ReadSnapshotAsync(gateway.RanchId, cancellationToken);
        return snapshot.Revision > afterRevision ? snapshot : null;
    }

    private async Task<long> AdvanceRevisionAsync(Guid ranchId, CancellationToken cancellationToken)
    {
        var ruleSet = await ruleSets.FindAsync(ranchId, cancellationToken);
        if (ruleSet is null)
        {
            ruleSet = new MonitoringRuleSet(ranchId);
            ruleSets.Add(ruleSet);
        }

        return ruleSet.Advance();
    }
}
