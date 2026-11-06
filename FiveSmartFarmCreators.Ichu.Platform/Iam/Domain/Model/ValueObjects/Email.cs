using System.Net.Mail;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;

/// <summary>Defines the Email contract.</summary>
public sealed record Email
{
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
