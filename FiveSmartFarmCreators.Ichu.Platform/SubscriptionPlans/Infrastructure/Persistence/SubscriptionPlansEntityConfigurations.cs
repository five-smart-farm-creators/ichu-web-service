using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Infrastructure.Persistence;

/// <summary>EF Core mappings for the subscription schema. Applied by the shared DbContext through ApplyConfigurationsFromAssembly.</summary>
public sealed class SubscriptionPlansEntityConfigurations :
    IEntityTypeConfiguration<Plan>,
    IEntityTypeConfiguration<Subscription>,
    IEntityTypeConfiguration<Payment>
{
    public const string Schema = "subscription";
    private const int EnumLength = 32;

    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.ToTable("Plans", Schema);
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();
        builder.Property(p => p.Code).HasMaxLength(Plan.MaximumCodeLength).IsRequired();
        builder.HasIndex(p => p.Code).IsUnique();
        builder.Property(p => p.Name).HasMaxLength(Plan.MaximumNameLength).IsRequired();
        builder.Property(p => p.BillingPeriod).HasConversion<string>().HasMaxLength(EnumLength);
        builder.ComplexProperty(p => p.PricePerHead, ConfigureMoney);
        builder.ComplexProperty(p => p.HardwareUnitPrice, ConfigureMoney);
        builder.OwnsMany(p => p.Features, feature =>
        {
            feature.ToTable("PlanFeatures", Schema);
            feature.WithOwner().HasForeignKey("PlanId");
            feature.Property(f => f.Code).HasConversion<string>().HasMaxLength(EnumLength);
            feature.HasKey("PlanId", "Code");
        });
        builder.Navigation(p => p.Features).UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("Subscriptions", Schema);
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();
        builder.Property(s => s.BillingPeriod).HasConversion<string>().HasMaxLength(EnumLength);
        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(EnumLength);
        builder.ComplexProperty(s => s.RecurringAmount, ConfigureMoney);
        builder.Property(s => s.TermsVersion).HasMaxLength(Subscription.MaximumTermsVersionLength).IsRequired();
        builder.Property(s => s.AcquisitionSource).HasMaxLength(Subscription.MaximumAcquisitionSourceLength);
        builder.Property(s => s.CancellationReason).HasMaxLength(Subscription.MaximumCancellationReasonLength);
        builder.Property(s => s.Version).IsConcurrencyToken();
        builder.HasIndex(s => s.OperationId).IsUnique();

        // A ranch has at most one pending or active subscription; cancelled ones remain as history.
        builder.HasIndex(s => s.RanchId)
            .IsUnique()
            .HasFilter("\"Status\" IN ('Pending', 'Active')")
            .HasDatabaseName("IX_Subscriptions_RanchId_Open");
        builder.HasOne<Plan>().WithMany().HasForeignKey(s => s.PlanId).OnDelete(DeleteBehavior.Restrict);

        // Payments belong to the aggregate; they are always loaded and saved through their subscription.
        builder.HasMany(s => s.Payments).WithOne().HasForeignKey("SubscriptionId").OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(s => s.Payments).UsePropertyAccessMode(PropertyAccessMode.Field).AutoInclude();
    }

    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments", Schema);
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();
        builder.Property(p => p.Purpose).HasConversion<string>().HasMaxLength(EnumLength);
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(EnumLength);
        builder.ComplexProperty(p => p.Amount, ConfigureMoney);
        builder.Property(p => p.ExternalReference).HasMaxLength(Payment.MaximumReferenceLength);
        builder.Property(p => p.RejectionReason).HasMaxLength(Payment.MaximumReasonLength);
    }

    private static void ConfigureMoney(ComplexPropertyBuilder<Money> money)
    {
        money.Property(m => m.Amount).HasPrecision(12, 2);
        money.Property(m => m.Currency).HasMaxLength(3).IsFixedLength().IsRequired();
    }
}
