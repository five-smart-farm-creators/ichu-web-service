using Microsoft.EntityFrameworkCore;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;

/// <summary>
/// Exposes module-owned mappings for registration in the host's shared EF model.
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    /// Adds animal, history, membership and pending-event mappings to the host model under the cattle schema.
    /// </summary>
    public static void ApplyCattleInformationConfiguration(this ModelBuilder builder)
    {
        builder.ApplyConfiguration(new CattleConfiguration());
        builder.ApplyConfiguration(new StagePeriodConfiguration());
        builder.ApplyConfiguration(new LotMembershipConfiguration());
        builder.ApplyConfiguration(new LotConfiguration());
        builder.ApplyConfiguration(new CattleIntegrationEventConfiguration());
    }
}
