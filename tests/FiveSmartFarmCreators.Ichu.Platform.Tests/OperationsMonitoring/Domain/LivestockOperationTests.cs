using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.OperationsMonitoring.Domain;

public class LivestockOperationTests
{
    private static readonly Guid RanchId = Guid.Parse("11111111-1111-4111-8111-111111111111");
    private static readonly Guid CattleId = Guid.Parse("7b2e3d1f-1e4c-4d62-8b88-2c3d4e5f6a71");
    private static readonly Guid AuthorId = Guid.Parse("44444444-4444-4444-8444-444444444444");
    private static readonly DateTimeOffset OccurredAt = new(2026, 11, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void InseminationKeepsItsTrimmedSireReferenceAndIsClinical()
    {
        var operation = Create(OperationType.Insemination, Details(sire: "  Bull 07 "));

        Assert.True(operation.IsSuccess);
        Assert.Equal("Bull 07", operation.Value!.Reproductive.SireReference);
        Assert.True(LivestockOperation.IsClinical(OperationType.Insemination));
    }

    [Fact]
    public void SireReferenceLongerThanTheLimitIsRejected()
    {
        var details = ReproductiveDetails.Create(new string('s', ReproductiveDetails.MaximumSireReferenceLength + 1), null, null);

        Assert.Equal(OperationsMonitoringErrors.SireReferenceTooLong.Code, details.DomainError.Code);
    }

    [Fact]
    public void NaturalMatingKeepsItsSireAsAFieldEvent()
    {
        var operation = Create(OperationType.NaturalMating, Details(sire: "Bull 12"));

        Assert.True(operation.IsSuccess);
        Assert.False(LivestockOperation.IsClinical(OperationType.NaturalMating));
    }

    [Fact]
    public void BirthRejectsReproductiveDetails()
    {
        var operation = Create(OperationType.Birth, Details(sire: "Bull 12"));

        Assert.Equal(OperationsMonitoringErrors.ReproductiveDetailsNotAllowed.Code, operation.DomainError.Code);
    }

    [Fact]
    public void HealthEventRejectsAPregnancyResult()
    {
        var operation = Create(OperationType.HealthEvent, Details(result: "Open"));

        Assert.Equal(OperationsMonitoringErrors.ReproductiveDetailsNotAllowed.Code, operation.DomainError.Code);
    }

    [Fact]
    public void OperationWithoutReproductiveFactsCarriesAnEmptyValue()
    {
        var operation = Create(OperationType.HealthEvent, Details());

        Assert.True(operation.IsSuccess);
        Assert.True(operation.Value!.Reproductive.IsEmpty);
    }

    [Fact]
    public void InseminationRejectsAPregnancyResult()
    {
        var operation = Create(OperationType.Insemination, Details(result: "Pregnant"));

        Assert.Equal(OperationsMonitoringErrors.ReproductiveDetailsNotAllowed.Code, operation.DomainError.Code);
    }

    [Fact]
    public void NaturalMatingRejectsAnExpectedCalvingDate()
    {
        var operation = Create(OperationType.NaturalMating, Details(expected: new DateOnly(2027, 1, 1)));

        Assert.Equal(OperationsMonitoringErrors.ReproductiveDetailsNotAllowed.Code, operation.DomainError.Code);
    }

    [Fact]
    public void PregnancyDiagnosisRequiresAResult()
    {
        var operation = Create(OperationType.PregnancyDiagnosis, Details());

        Assert.Equal(OperationsMonitoringErrors.PregnancyResultRequired.Code, operation.DomainError.Code);
    }

    [Fact]
    public void PregnancyDiagnosisRejectsASireReference()
    {
        var operation = Create(OperationType.PregnancyDiagnosis, Details(sire: "Bull 07", result: "Pregnant"));

        Assert.Equal(OperationsMonitoringErrors.ReproductiveDetailsNotAllowed.Code, operation.DomainError.Code);
    }

    [Theory]
    [InlineData("Maybe")]
    [InlineData("0")]
    public void ResultMustBeTheNameOfAnOutcome(string result)
    {
        var details = ReproductiveDetails.Create(null, result, null);

        Assert.Equal(OperationsMonitoringErrors.PregnancyResultUnknown.Code, details.DomainError.Code);
    }

    [Fact]
    public void ResultNameIsReadWithoutRegardToCase()
    {
        var details = ReproductiveDetails.Create(null, "pregnant", null);

        Assert.Equal(PregnancyResult.Pregnant, details.Value!.PregnancyResult);
    }

    [Fact]
    public void PregnantDiagnosisWithoutCalvingDateIsAccepted()
    {
        var operation = Create(OperationType.PregnancyDiagnosis, Details(result: "Pregnant"));

        Assert.True(operation.IsSuccess);
        Assert.Null(operation.Value!.Reproductive.ExpectedCalvingDate);
    }

    [Fact]
    public void CalvingDateWithinThreeHundredDaysOfThePregnantDiagnosisIsAccepted()
    {
        // 2027-08-28 is exactly 300 days after the diagnosis on 2026-11-01.
        var operation = Create(OperationType.PregnancyDiagnosis, Details(result: "Pregnant", expected: new DateOnly(2027, 8, 28)));

        Assert.True(operation.IsSuccess);
    }

    [Fact]
    public void CalvingDateOnTheDiagnosisDayIsRejected()
    {
        var operation = Create(OperationType.PregnancyDiagnosis, Details(result: "Pregnant", expected: new DateOnly(2026, 11, 1)));

        Assert.Equal(OperationsMonitoringErrors.ExpectedCalvingDateInvalid.Code, operation.DomainError.Code);
    }

    [Fact]
    public void CalvingDateMoreThanThreeHundredDaysAfterTheDiagnosisIsRejected()
    {
        var operation = Create(OperationType.PregnancyDiagnosis, Details(result: "Pregnant", expected: new DateOnly(2027, 8, 29)));

        Assert.Equal(OperationsMonitoringErrors.ExpectedCalvingDateInvalid.Code, operation.DomainError.Code);
    }

    [Fact]
    public void OpenDiagnosisRejectsACalvingDate()
    {
        var operation = Create(OperationType.PregnancyDiagnosis, Details(result: "Open", expected: new DateOnly(2027, 1, 1)));

        Assert.Equal(OperationsMonitoringErrors.ExpectedCalvingDateInvalid.Code, operation.DomainError.Code);
    }

    [Fact]
    public void PregnancyDiagnosisIsClinicalAndHeatIsAReproductiveFieldEvent()
    {
        Assert.True(LivestockOperation.IsClinical(OperationType.PregnancyDiagnosis));
        Assert.False(LivestockOperation.IsClinical(OperationType.Abortion));
        Assert.True(LivestockOperation.IsReproductive(OperationType.HeatObserved));
        Assert.False(LivestockOperation.IsReproductive(OperationType.Vaccination));
    }

    private static Result<LivestockOperation> Create(OperationType type, Result<ReproductiveDetails> details) =>
        details.IsFailure
            ? Result<LivestockOperation>.Failure(details.DomainError)
            : LivestockOperation.Create(Guid.NewGuid(), RanchId, CattleId, type, OccurredAt, AuthorId, null, null, null, OccurredAt,
                details.Value!);

    private static Result<ReproductiveDetails> Details(string? sire = null, string? result = null, DateOnly? expected = null) =>
        ReproductiveDetails.Create(sire, result, expected);
}
