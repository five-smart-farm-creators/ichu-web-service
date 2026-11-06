using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

internal sealed class FollowUpConfiguration : IEntityTypeConfiguration<FollowUp>
{
    public void Configure(EntityTypeBuilder<FollowUp> builder)
    {
        builder.ToTable("follow_ups");
        builder.HasKey(followUp => followUp.Id);

        builder.Property(followUp => followUp.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();
        builder.Property(followUp => followUp.ActivityId)
            .HasColumnName("activity_id")
            .IsRequired();
        builder.Property(followUp => followUp.DueAt)
            .HasColumnName("due_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasOne<PlannedActivity>()
            .WithMany()
            .HasForeignKey(followUp => followUp.ActivityId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(followUp => new { followUp.ActivityId, followUp.DueAt })
            .HasDatabaseName("ix_follow_ups_activity_id_due_at");
    }
}
