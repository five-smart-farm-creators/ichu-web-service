using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

internal sealed class PlannedActivityConfiguration : IEntityTypeConfiguration<PlannedActivity>
{
    public void Configure(EntityTypeBuilder<PlannedActivity> builder)
    {
        builder.ToTable("planned_activities");
        builder.HasKey(activity => activity.Id);

        builder.Property(activity => activity.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();
        builder.Property(activity => activity.RanchId)
            .HasColumnName("ranch_id")
            .IsRequired();
        builder.Property(activity => activity.CampaignId)
            .HasColumnName("campaign_id");
        builder.Property(activity => activity.CattleId)
            .HasColumnName("cattle_id");
        builder.Property(activity => activity.Title)
            .HasColumnName("title")
            .IsRequired();
        builder.Property(activity => activity.ScheduledAt)
            .HasColumnName("scheduled_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasOne<PlanningCampaign>()
            .WithMany()
            .HasForeignKey(activity => activity.CampaignId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(activity => new { activity.RanchId, activity.ScheduledAt })
            .HasDatabaseName("ix_planned_activities_ranch_id_scheduled_at");
        builder.HasIndex(activity => activity.CampaignId)
            .HasDatabaseName("ix_planned_activities_campaign_id");
    }
}
