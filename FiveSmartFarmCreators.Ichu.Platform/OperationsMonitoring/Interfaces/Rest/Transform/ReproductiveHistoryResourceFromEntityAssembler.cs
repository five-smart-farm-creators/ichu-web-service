using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

public static class ReproductiveHistoryResourceFromEntityAssembler
{
    public static ReproductiveHistoryResource ToResourceFromEntity(ReproductiveHistoryReport report)
    {
        var history = report.History;
        return new ReproductiveHistoryResource(
            report.Animal.CattleId,
            report.Animal.Sex.ToString(),
            report.Animal.LifeStage,
            report.AgeInMonths,
            history.HasHistory,
            history.Services.Select(ToServiceResource).ToList(),
            history.PregnancyDiagnoses.Select(ToDiagnosisResource).ToList(),
            history.Calvings.Select(ToEventResource).ToList(),
            history.Abortions.Select(ToEventResource).ToList(),
            history.HeatsObserved.Select(ToEventResource).ToList(),
            history.Intervals.Select(interval => new ReproductiveIntervalResource(
                interval.CalvingAt, interval.ConceptionServiceAt, interval.DaysOpen)).ToList(),
            history.ServicesPerPregnancy,
            history.PregnanciesConfirmed);
    }

    private static ReproductiveServiceResource ToServiceResource(LivestockOperation service) =>
        new(service.OccurredAt, service.Type.ToString(), service.Reproductive.SireReference);

    // Every diagnosis stores a result, because the aggregate rejects a diagnosis without one.
    private static PregnancyDiagnosisResource ToDiagnosisResource(LivestockOperation diagnosis) =>
        new(diagnosis.OccurredAt, diagnosis.Reproductive.PregnancyResult!.Value.ToString(), diagnosis.Reproductive.ExpectedCalvingDate);

    private static ReproductiveEventResource ToEventResource(LivestockOperation operation) => new(operation.OccurredAt);
}
