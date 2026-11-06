using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

internal sealed class PlanningCampaignConfiguration : IEntityTypeConfiguration<PlanningCampaign>
{
    public void Configure(EntityTypeBuilder<PlanningCampaign> builder)
    {
        builder.ToTable("planning_campaigns");
        builder.HasKey(campaign => campaign.Id);

        builder.Property(campaign => campaign.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();
        builder.Property(campaign => campaign.RanchId)
            .HasColumnName("ranch_id")
            .IsRequired();
        builder.Property(campaign => campaign.Name)
            .HasColumnName("name")
            .IsRequired();
        builder.Property(campaign => campaign.Kind)
            .HasColumnName("kind")
            .HasConversion<int>()
            .IsRequired();
        builder.Property(campaign => campaign.StartsOn)
            .HasColumnName("starts_on")
            .IsRequired();
        builder.Property(campaign => campaign.EndsOn)
            .HasColumnName("ends_on")
            .IsRequired();

        builder.HasIndex(campaign => new { campaign.RanchId, campaign.StartsOn })
            .HasDatabaseName("ix_planning_campaigns_ranch_id_starts_on");
    }
}
