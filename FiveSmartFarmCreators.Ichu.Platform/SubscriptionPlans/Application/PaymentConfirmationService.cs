using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application.Ports;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Repositories;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application;

/// <summary>Applies verified payment results. This is the only path from Pending to Active.</summary>
public sealed class PaymentConfirmationService(
    IPaymentNotificationVerifier verifier,
    ISubscriptionRepository subscriptions,
    ISubscriptionPlansUnitOfWork unitOfWork)
{
    public async Task<ChangeOutcome> ProcessAsync(ProcessPaymentNotificationCommand command, CancellationToken cancellationToken)
    {
        var result = await verifier.VerifyAsync(command.Payload, command.Headers, cancellationToken)
            ?? throw SubscriptionException.Unauthenticated("The payment notification could not be verified.");
        var subscription = await subscriptions.FindByPaymentAsync(result.PaymentId, cancellationToken)
            ?? throw SubscriptionException.NotFound("The payment does not exist.");

        var outcome = result.Accepted
            ? subscription.ConfirmPayment(result.PaymentId, result.ExternalReference, new Money(result.Amount, result.Currency), result.ProcessedAt)
            : subscription.RejectPayment(result.PaymentId, result.ExternalReference, result.RejectionReason, result.ProcessedAt);
        if (outcome == ChangeOutcome.Applied)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return outcome;
    }
}
