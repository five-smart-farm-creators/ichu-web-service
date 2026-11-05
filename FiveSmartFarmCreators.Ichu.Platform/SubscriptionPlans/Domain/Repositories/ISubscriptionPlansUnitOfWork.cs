namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Repositories;

public interface ISubscriptionPlansUnitOfWork
{
    /// <summary>Persists pending changes; concurrent or duplicate writes surface as a conflict.</summary>
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
