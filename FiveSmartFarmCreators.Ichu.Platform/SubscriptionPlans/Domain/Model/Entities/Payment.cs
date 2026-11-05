using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Entities;

/// <summary>A payment requested for a subscription period. Only a verified provider result changes its status.</summary>
public sealed class Payment
{
    public const int MaximumReferenceLength = 120;
    public const int MaximumReasonLength = 200;

    public Guid Id { get; private set; }
    public PaymentPurpose Purpose { get; private set; }
    public Money Amount { get; private set; } = null!;
    public PaymentStatus Status { get; private set; }
    public DateTimeOffset RequestedAt { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }
    public string? ExternalReference { get; private set; }
    public string? RejectionReason { get; private set; }

    private Payment()
    {
    }

    public Payment(PaymentPurpose purpose, Money amount, DateTimeOffset requestedAt)
    {
        Id = Guid.NewGuid();
        Purpose = purpose;
        Amount = amount;
        Status = PaymentStatus.Pending;
        RequestedAt = requestedAt.ToUniversalTime();
    }

    public void MarkAccepted(string externalReference, DateTimeOffset processedAt)
    {
        ExternalReference = RequiredText.Normalize(externalReference, MaximumReferenceLength, "external reference");
        Status = PaymentStatus.Accepted;
        ProcessedAt = processedAt.ToUniversalTime();
    }

    public void MarkRejected(string externalReference, string? reason, DateTimeOffset processedAt)
    {
        ExternalReference = RequiredText.Normalize(externalReference, MaximumReferenceLength, "external reference");
        RejectionReason = string.IsNullOrWhiteSpace(reason) ? null : RequiredText.Normalize(reason, MaximumReasonLength, "rejection reason");
        Status = PaymentStatus.Rejected;
        ProcessedAt = processedAt.ToUniversalTime();
    }
}
