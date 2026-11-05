using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

public sealed record EdgeBatchResource(
    IReadOnlyList<EdgeReadingResource>? Readings,
    IReadOnlyList<EdgeAlertResource>? Alerts,
    IReadOnlyList<EdgeAlertChangeResource>? AlertChanges)
{
    public SynchronizeEdgeBatchCommand ToCommand() =>
        new(
            (Readings ?? []).Select(r => r.ToEntry()).ToList(),
            (Alerts ?? []).Select(a => a.ToEntry()).ToList(),
            (AlertChanges ?? []).Select(c => c.ToEntry()).ToList());
}
