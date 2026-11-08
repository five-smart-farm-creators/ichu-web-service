using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.OperationsMonitoring.Domain;

public class MonitoringThresholdTests
{
    [Fact]
    public void RangeWhoseMinimumIsNotBelowItsMaximumIsRejected()
    {
        var range = ThresholdRange.Create(39, 39);

        Assert.Equal(OperationsMonitoringErrors.ThresholdRangeInvalid.Code, range.DomainError.Code);
    }

    [Fact]
    public void RangeReportsOnlyValuesOutsideItsBounds()
    {
        var range = ThresholdRange.Create(35, 39).Value!;

        Assert.True(range.IsExceededBy(34.9));
        Assert.False(range.IsExceededBy(37));
        Assert.True(range.IsExceededBy(39.1));
    }

    [Fact]
    public void LifeStageIsTrimmedAndMustFitTheLimit()
    {
        Assert.Equal("Heifer", MonitoringThreshold.NormalizeLifeStage("  Heifer  ").Value);
        Assert.Equal(OperationsMonitoringErrors.ThresholdLifeStageLength.Code, MonitoringThreshold.NormalizeLifeStage("   ").DomainError.Code);
        Assert.Equal(OperationsMonitoringErrors.ThresholdLifeStageLength.Code,
            MonitoringThreshold.NormalizeLifeStage(new string('x', MonitoringThreshold.MaximumLifeStageLength + 1)).DomainError.Code);
    }

    [Fact]
    public void ThresholdRequiresItsRanch()
    {
        var threshold = MonitoringThreshold.Create(Guid.Empty, "Heifer", MonitoredMetric.BodyTemperature,
            ThresholdRange.Create(35, 39).Value!, AlertSeverity.Warning, 1);

        Assert.Equal(OperationsMonitoringErrors.RanchIdentifierRequired.Code, threshold.DomainError.Code);
    }

    [Fact]
    public void ReconfiguringKeepsTheIdentityAndReplacesTheRange()
    {
        var threshold = MonitoringThreshold.Create(TelemetryFixtures.RanchId, "Heifer", MonitoredMetric.BodyTemperature,
            ThresholdRange.Create(35, 39).Value!, AlertSeverity.Warning, 1).Value!;
        var identity = threshold.Id;

        threshold.Reconfigure(ThresholdRange.Create(35.5, 38.5).Value!, AlertSeverity.Critical, 2);

        Assert.Equal(identity, threshold.Id);
        Assert.Equal(38.5, threshold.Range.MaxValue);
        Assert.Equal(2, threshold.Revision);
    }
}
