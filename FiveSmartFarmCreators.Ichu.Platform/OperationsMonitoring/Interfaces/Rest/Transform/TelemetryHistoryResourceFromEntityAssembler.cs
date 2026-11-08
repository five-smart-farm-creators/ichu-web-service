using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

public static class TelemetryHistoryResourceFromEntityAssembler
{
    public static TelemetryHistoryResource ToResourceFromEntity(TelemetryHistory history) =>
        new(
            history.AnimalId,
            history.Period.From,
            history.Period.To,
            history.OldestRetainedRecordedAt,
            history.Readings.Page,
            history.Readings.PageSize,
            history.Readings.TotalCount,
            history.Readings.Items.Select(TelemetryReadingResourceFromEntityAssembler.ToResourceFromEntity).ToList());
}
