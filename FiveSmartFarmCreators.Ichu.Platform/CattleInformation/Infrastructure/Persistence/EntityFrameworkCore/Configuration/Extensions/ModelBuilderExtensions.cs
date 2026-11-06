using Microsoft.EntityFrameworkCore;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;

public static class ModelBuilderExtensions
{
    public static void ApplyCattleInformationConfiguration(this ModelBuilder builder)
    {
        builder.ApplyConfiguration(new CattleConfiguration());
        builder.ApplyConfiguration(new StagePeriodConfiguration());
        builder.ApplyConfiguration(new LotMembershipConfiguration());
        builder.ApplyConfiguration(new LotConfiguration());
        builder.ApplyConfiguration(new CattleIntegrationEventConfiguration());
    }
}
