using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Commands;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Interfaces.Rest.Resources;

/// <summary>Requests a subscription from a current quote and the accepted terms version.</summary>
public sealed record RequestSubscriptionResource(
    Guid RanchId,
    Guid PlanId,
    int? HeadCount,
    Guid QuoteId,
    string? TermsVersion,
    Guid OperationId,
    string? AcquisitionSource)
{
    public RequestSubscriptionCommand ToCommand()
    {
        var headCount = HeadCount ?? throw SubscriptionException.InvalidInput("The head count is required.");
        if (string.IsNullOrWhiteSpace(TermsVersion))
        {
            throw SubscriptionException.InvalidInput("The accepted terms version is required.");
        }

        if (OperationId == Guid.Empty || QuoteId == Guid.Empty)
        {
            throw SubscriptionException.InvalidInput("The operation and quote identifiers are required.");
        }

        return new RequestSubscriptionCommand(RanchId, PlanId, headCount, QuoteId, TermsVersion, OperationId, AcquisitionSource);
    }
}
