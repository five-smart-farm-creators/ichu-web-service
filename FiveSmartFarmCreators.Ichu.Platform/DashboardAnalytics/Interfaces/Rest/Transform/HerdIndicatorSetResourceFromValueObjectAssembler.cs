using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Transform;

/// <summary>
///     Maps HerdIndicatorSet values to the REST response contract.
/// </summary>
public static class HerdIndicatorSetResourceFromValueObjectAssembler
{
    /// <summary>
    ///     Copies analytics values into the response without recalculating domain results.
    /// </summary>
    public static HerdIndicatorSetResource ToResourceFromValueObject(HerdIndicatorSet value) =>
        new(value.RanchId, DateRangeResourceFromValueObjectAssembler.ToResourceFromValueObject(value.Period),
            value.UpdatedAt, value.Source,
            IndicatorValueResourceFromValueObjectAssembler.ToResourceFromValueObject(value.MonitoredAnimals),
            value.OpenAlertsByType, value.AlertsUnavailableReason,
            IndicatorValueResourceFromValueObjectAssembler.ToResourceFromValueObject(value.OutsideGeofenceAnimals),
            IndicatorValueResourceFromValueObjectAssembler.ToResourceFromValueObject(value.MortalityRate),
            IndicatorValueResourceFromValueObjectAssembler.ToResourceFromValueObject(value.Births),
            IndicatorValueResourceFromValueObjectAssembler.ToResourceFromValueObject(value.ConfirmedPregnancies),
            value.ExcludedAnimals, value.MissingReadings, value.ReadingsCoverageReason);
}
