using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

/// <summary>
///     The breeding history of one female, derived from its operations in occurrence order. Males have no history, because reproductive events are recorded only for females.
/// </summary>
public sealed record ReproductiveHistory(
    IReadOnlyList<LivestockOperation> Services,
    IReadOnlyList<LivestockOperation> PregnancyDiagnoses,
    IReadOnlyList<LivestockOperation> Calvings,
    IReadOnlyList<LivestockOperation> Abortions,
    IReadOnlyList<LivestockOperation> HeatsObserved,
    IReadOnlyList<ReproductiveInterval> Intervals,
    int PregnanciesConfirmed,
    double? ServicesPerPregnancy)
{
    /// <summary>
    ///     A heat alone does not start a history, because it is only an observation of the cycle.
    /// </summary>
    public bool HasHistory => Services.Count > 0 || Calvings.Count > 0;
}
