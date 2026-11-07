using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

internal sealed class WithdrawalPeriodConfiguration : IEntityTypeConfiguration<WithdrawalPeriod>
{
    public void Configure(EntityTypeBuilder<WithdrawalPeriod> builder)
    {
        builder.ToTable("withdrawal_periods");
        builder.HasKey(period => period.Id);

        builder.Property(period => period.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();
        builder.Property(period => period.RanchId)
            .HasColumnName("ranch_id")
            .IsRequired();
        builder.Property(period => period.CattleId)
            .HasColumnName("cattle_id")
            .IsRequired();
        builder.Property(period => period.InterventionId)
            .HasColumnName("intervention_id");
        builder.Property(period => period.StartsAt)
            .HasColumnName("starts_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();
        builder.Property(period => period.EndsAt)
            .HasColumnName("ends_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasIndex(period => new { period.RanchId, period.CattleId, period.StartsAt })
            .HasDatabaseName("ix_withdrawal_periods_ranch_cattle_starts_at");
        builder.HasIndex(period => period.InterventionId)
            .HasDatabaseName("ix_withdrawal_periods_intervention_id");
    }
}
