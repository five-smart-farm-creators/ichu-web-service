using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application;
using Microsoft.Extensions.Options;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Infrastructure.Simulation;

/// <summary>
///     Gives the simulated ranch of the Operations and Monitoring module a Standard subscription in Development, so that
///     plan-gated features can be exercised before the payment phase exists.
/// </summary>
public sealed class DevelopmentSubscriptionSeeder(
    IPlanRepository plans,
    ISubscriptionRepository subscriptions,
    ISubscriptionCommandService commands,
    IUnitOfWork unitOfWork,
    IOptions<SubscriptionPlansOptions> options,
    TimeProvider clock)
{
    // Must match the ranch of the Operations and Monitoring simulation in appsettings.Development.json.
    private static readonly Guid SimulatedRanchId = Guid.Parse("11111111-1111-4111-8111-111111111111");
    private const int SimulatedHeadCount = 120;

    /// <summary>
    ///     Requests and activates a Standard subscription unless the ranch already has a pending or active one. The
    ///     request skips the access check because no HTTP account exists at startup.
    /// </summary>
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (await subscriptions.FindCurrentByRanchAsync(SimulatedRanchId, cancellationToken) is not null)
        {
            return;
        }

        var standard = (await plans.ListPublishedAsync(cancellationToken))
            .SingleOrDefault(plan => plan.Tier == PlanTier.Standard)
            ?? throw new InvalidOperationException("The development seed needs the published Standard plan.");
        var quote = standard.EstimateFor(new HeadCount(SimulatedHeadCount));
        if (quote.IsFailure)
        {
            throw new InvalidOperationException(quote.Message);
        }

        var subscription = Subscription.Request(SimulatedRanchId, quote.Value!, options.Value.CurrentTermsVersion,
            Guid.NewGuid(), "development-seed", clock.GetUtcNow());
        await subscriptions.AddAsync(subscription, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        var activation = await commands.Handle(new ActivateSubscriptionCommand(subscription.Id), cancellationToken);
        if (activation.IsFailure)
        {
            throw new InvalidOperationException(activation.Message);
        }
    }
}
