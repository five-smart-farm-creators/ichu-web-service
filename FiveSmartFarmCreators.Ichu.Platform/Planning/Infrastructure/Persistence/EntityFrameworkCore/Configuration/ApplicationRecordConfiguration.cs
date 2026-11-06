using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

internal sealed class ApplicationRecordConfiguration : IEntityTypeConfiguration<ApplicationRecord>
{
    public void Configure(EntityTypeBuilder<ApplicationRecord> builder)
    {
        builder.ToTable("application_records");
        builder.HasKey(application => application.Id);

        builder.Property(application => application.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();
        builder.Property(application => application.ActivityId)
            .HasColumnName("activity_id")
            .IsRequired();
        builder.Property(application => application.CattleId)
            .HasColumnName("cattle_id")
            .IsRequired();
        builder.Property(application => application.AppliedAt)
            .HasColumnName("applied_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();
        builder.Property(application => application.InterventionId)
            .HasColumnName("intervention_id");

        builder.HasOne<PlannedActivity>()
            .WithMany()
            .HasForeignKey(application => application.ActivityId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(application => new { application.ActivityId, application.AppliedAt })
            .HasDatabaseName("ix_application_records_activity_id_applied_at");
        builder.HasIndex(application => application.InterventionId)
            .HasDatabaseName("ix_application_records_intervention_id");
    }
}
