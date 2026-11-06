using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

/// <summary>
///     Assembles HTTP response resources from <see cref="TelemetryHistory"/> value objects.
/// </summary>
public static class TelemetryHistoryResourceFromEntityAssembler
{
    /// <summary>
    ///     Transforms a <see cref="TelemetryHistory"/> into a <see cref="TelemetryHistoryResource"/>.
    /// </summary>
    /// <param name="history">The readings of an animal for a period.</param>
    /// <returns>A resource representation suitable for HTTP responses.</returns>
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
