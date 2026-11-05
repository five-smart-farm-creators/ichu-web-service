namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Commands;

public sealed record RequestSubscriptionCommand(
    Guid RanchId,
    Guid PlanId,
    int HeadCount,
    Guid QuoteId,
    string TermsVersion,
    Guid OperationId,
    string? AcquisitionSource);
