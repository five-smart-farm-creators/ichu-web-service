using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

/// <summary>
///     The reproductive facts sent with a breeding service or a pregnancy diagnosis. The operation type decides which of them it accepts.
/// </summary>
public sealed record ReproductiveDetails
{
    public const int MaximumSireReferenceLength = 80;

    private ReproductiveDetails(string? sireReference, PregnancyResult? pregnancyResult, DateOnly? expectedCalvingDate)
    {
        SireReference = sireReference;
        PregnancyResult = pregnancyResult;
        ExpectedCalvingDate = expectedCalvingDate;
    }

    public string? SireReference { get; }

    public PregnancyResult? PregnancyResult { get; }

    public DateOnly? ExpectedCalvingDate { get; }

    /// <summary>
    ///     Most operations carry no reproductive facts, so the empty value stands for their absence and keeps the mapping a required complex property.
    /// </summary>
    public bool IsEmpty => SireReference is null && PregnancyResult is null && ExpectedCalvingDate is null;

    public static Result<ReproductiveDetails> Create(string? sireReference, string? pregnancyResult, DateOnly? expectedCalvingDate)
    {
        var sire = string.IsNullOrWhiteSpace(sireReference) ? null : sireReference.Trim();
        if (sire is not null && sire.Length > MaximumSireReferenceLength)
        {
            return Result<ReproductiveDetails>.Failure(OperationsMonitoringErrors.SireReferenceTooLong);
        }

        var result = ParsePregnancyResult(pregnancyResult);
        if (result.IsFailure)
        {
            return Result<ReproductiveDetails>.Failure(result.DomainError);
        }

        return Result<ReproductiveDetails>.Success(new ReproductiveDetails(sire, result.Value, expectedCalvingDate));
    }

    // Only the names are accepted, so a numeric text cannot select a result by its position.
    private static Result<PregnancyResult?> ParsePregnancyResult(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<PregnancyResult?>.Success(null);
        }

        var name = Enum.GetNames<PregnancyResult>()
            .FirstOrDefault(candidate => candidate.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
        return name is null
            ? Result<PregnancyResult?>.Failure(OperationsMonitoringErrors.PregnancyResultUnknown)
            : Result<PregnancyResult?>.Success(Enum.Parse<PregnancyResult>(name));
    }
}
