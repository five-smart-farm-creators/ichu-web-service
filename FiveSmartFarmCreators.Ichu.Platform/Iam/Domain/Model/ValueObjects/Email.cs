using System.Net.Mail;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;

/// <summary>
///     Validated email address, stored in lowercase so that account lookups ignore case.
/// </summary>
public sealed record Email
{
    private const int MaximumLength = 254;

    /// <summary>
    ///     Rebuilds an address that was validated when it was stored. Requests must use <see cref="Create"/>.
    /// </summary>
    public Email(string value)
    {
        Value = Normalize(value) ?? throw new ArgumentException("A valid email address is required.", nameof(value));
    }

    public string Value { get; }

    public static Result<Email> Create(string value) =>
        Normalize(value) is null
            ? Result<Email>.Failure(IamErrors.InvalidEmail)
            : Result<Email>.Success(new Email(value));

    private static string? Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > MaximumLength)
        {
            return null;
        }

        var trimmed = value.Trim();
        if (!MailAddress.TryCreate(trimmed, out var address) || address.Address != trimmed)
        {
            return null;
        }

        return address.Address.ToLowerInvariant();
    }
}
