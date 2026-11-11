using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;

/// <summary>
///     The alphabet leaves out I, O, 0 and 1, so a code read aloud or typed by hand stays unambiguous.
/// </summary>
public sealed record RanchAccessCode
{
    public const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    public const int Length = 8;

    /// <summary>
    ///     Rebuilds a code that was validated when it was issued. Requests must use <see cref="Create"/>.
    /// </summary>
    public RanchAccessCode(string value)
    {
        Value = Normalize(value) ?? throw new ArgumentException("A valid ranch access code is required.", nameof(value));
    }

    public string Value { get; }

    public static Result<RanchAccessCode> Create(string value) =>
        Normalize(value) is null
            ? Result<RanchAccessCode>.Failure(IamErrors.InvalidRanchAccessCode)
            : Result<RanchAccessCode>.Success(new RanchAccessCode(value));

    private static string? Normalize(string value)
    {
        var candidate = value.Trim().ToUpperInvariant();
        return candidate.Length == Length && candidate.All(symbol => Alphabet.Contains(symbol)) ? candidate : null;
    }
}
