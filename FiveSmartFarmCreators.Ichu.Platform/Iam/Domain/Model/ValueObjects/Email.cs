using System.Net.Mail;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;

/// <summary>
///     Validates an account email address and stores it in lowercase.
/// </summary>
public sealed record Email
{
    /// <summary>
    ///     Trims and lowercases a valid email address.
    /// </summary>
    /// <param name="value">
    ///     The email address to validate.
    /// </param>
    /// <exception cref="ArgumentException">
    ///     <paramref name="value"/> is blank, too long, or not a valid email address.
    /// </exception>
    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 254 ||
            !MailAddress.TryCreate(value.Trim(), out var address) || address.Address != value.Trim())
        {
            throw new ArgumentException("A valid email address is required.", nameof(value));
        }

        Value = address.Address.ToLowerInvariant();
    }

    public string Value { get; }

}
