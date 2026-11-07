using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

/// <summary>
/// Enforces same-ranch membership through composite foreign keys and permits client-side orphan removal.
/// </summary>
public sealed class LotMembershipConfiguration : IEntityTypeConfiguration<LotMembership>
{
    public void Configure(EntityTypeBuilder<LotMembership> builder)
    {
        builder.ToTable("lot_memberships", "cattle");
        builder.HasKey(membership => new { membership.CattleId, membership.LotId });
        builder.Property(membership => membership.CattleId).HasColumnName("cattle_id");
        builder.Property(membership => membership.LotId).HasColumnName("lot_id");
        builder.Property(membership => membership.RanchId).HasColumnName("ranch_id").IsRequired();
        builder.Property(membership => membership.AssignedAt)
            .HasColumnName("assigned_at").HasColumnType("timestamp with time zone").IsRequired();

        builder.HasOne<Cattle>()
            .WithMany()
            .HasForeignKey(membership => new { membership.RanchId, membership.CattleId })
            .HasPrincipalKey(cattle => new { cattle.RanchId, cattle.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Lot>()
            .WithMany(lot => lot.Memberships)
            .HasForeignKey(membership => new { membership.RanchId, membership.LotId })
            .HasPrincipalKey(lot => new { lot.RanchId, lot.Id })
            .OnDelete(DeleteBehavior.ClientCascade);
    }
}
