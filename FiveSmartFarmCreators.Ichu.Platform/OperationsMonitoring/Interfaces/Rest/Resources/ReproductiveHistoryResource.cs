namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

/// <summary>
///     The breeding history of an animal. Males report no history, and their age and stage are still given.
/// </summary>
public record ReproductiveHistoryResource(
    Guid CattleId,
    string Sex,
    string LifeStage,
    int AgeInMonths,
    bool HasHistory,
    IReadOnlyList<ReproductiveServiceResource> Services,
    IReadOnlyList<PregnancyDiagnosisResource> PregnancyDiagnoses,
    IReadOnlyList<ReproductiveEventResource> Calvings,
    IReadOnlyList<ReproductiveEventResource> Abortions,
    IReadOnlyList<ReproductiveEventResource> HeatsObserved,
    IReadOnlyList<ReproductiveIntervalResource> Intervals,
    double? ServicesPerPregnancy,
    int PregnanciesConfirmed);
