using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration.ValueConverters;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;

/// <summary>
///     Entity Framework Core mappings of the Subscription Plans context. The plan catalog is seeded with the schema so
///     that every environment offers the same plans.
/// </summary>
public static class ModelBuilderExtensions
{
    private const string Schema = "subscription";
    private const string Currency = "PEN";
    private const decimal DeviceUnitPriceAmount = 120m;
    private const int EnumLength = 32;
    private const int MaximumPlanNameLength = 40;
    private const int MaximumTermsVersionLength = 16;
    private const int MaximumAcquisitionSourceLength = 64;

    private static readonly Guid BasicPlanId = Guid.Parse("5b1f0d2e-0c11-4a51-9b01-000000000001");
    private static readonly Guid StandardPlanId = Guid.Parse("5b1f0d2e-0c11-4a51-9b01-000000000002");
    private static readonly Guid ProPlanId = Guid.Parse("5b1f0d2e-0c11-4a51-9b01-000000000003");

    /// <summary>
    ///     Applies the mappings of the plans and subscriptions. The shared database context calls it from OnModelCreating.
    /// </summary>
    public static void ApplySubscriptionPlansConfiguration(this ModelBuilder builder)
    {
        ConfigurePlan(builder.Entity<Plan>());
        ConfigureSubscription(builder.Entity<Subscription>());
    }

    private static void ConfigurePlan(EntityTypeBuilder<Plan> plan)
    {
        plan.ToTable("plans", Schema);
        plan.HasKey(p => p.Id);
        plan.Property(p => p.Id)
            .HasConversion(PlanIdConverter())
            .ValueGeneratedNever()
            .HasColumnName("id");
        plan.Property(p => p.Tier).HasConversion<string>().HasMaxLength(EnumLength).HasColumnName("tier");
        plan.Property(p => p.Name).HasMaxLength(MaximumPlanNameLength).IsRequired().HasColumnName("name");
        plan.Property(p => p.DeviceLimit)
            .HasConversion(limit => limit.Maximum, maximum => new DeviceLimit(maximum))
            .HasColumnName("device_limit");
        plan.OwnsOne(p => p.AnnualFee, money =>
        {
            ConfigureMoney(money, "annual_fee");
            money.HasData(
                MoneyRow(BasicPlanId, 600m),
                MoneyRow(StandardPlanId, 1800m),
                MoneyRow(ProPlanId, 3900m));
        });
        plan.OwnsOne(p => p.DeviceUnitPrice, money =>
        {
            ConfigureMoney(money, "device_unit_price");
            money.HasData(
                MoneyRow(BasicPlanId, DeviceUnitPriceAmount),
                MoneyRow(StandardPlanId, DeviceUnitPriceAmount),
                MoneyRow(ProPlanId, DeviceUnitPriceAmount));
        });
        plan.Property(p => p.IsPublished).HasColumnName("is_published");
        plan.Ignore(p => p.Features);

        plan.OwnsMany<Plan.IncludedFeature>("_features", features =>
        {
            features.ToTable("plan_features", Schema);
            features.Property<PlanId>("plan_id").HasConversion(PlanIdConverter()).HasColumnName("plan_id");
            features.WithOwner().HasForeignKey("plan_id");
            features.HasKey("plan_id", nameof(Plan.IncludedFeature.Code));
            features.Property(f => f.Code).HasConversion<string>().HasMaxLength(EnumLength).HasColumnName("feature");
            features.HasData(
                FeatureRow(BasicPlanId, FeatureCode.Monitoring),
                FeatureRow(BasicPlanId, FeatureCode.Alerts),
                FeatureRow(StandardPlanId, FeatureCode.Monitoring),
                FeatureRow(StandardPlanId, FeatureCode.Alerts),
                FeatureRow(StandardPlanId, FeatureCode.Planning),
                FeatureRow(StandardPlanId, FeatureCode.Analytics),
                FeatureRow(ProPlanId, FeatureCode.Monitoring),
                FeatureRow(ProPlanId, FeatureCode.Alerts),
                FeatureRow(ProPlanId, FeatureCode.Planning),
                FeatureRow(ProPlanId, FeatureCode.Analytics),
                FeatureRow(ProPlanId, FeatureCode.ReportExport));
        });
        plan.Navigation("_features").UsePropertyAccessMode(PropertyAccessMode.Field);

        plan.HasData(
            CatalogPlan(BasicPlanId, PlanTier.Basic, "Basic", 50),
            CatalogPlan(StandardPlanId, PlanTier.Standard, "Standard", 200),
            CatalogPlan(ProPlanId, PlanTier.Pro, "Pro", 500));
    }

    private static void ConfigureSubscription(EntityTypeBuilder<Subscription> subscription)
    {
        subscription.ToTable("subscriptions", Schema);
        subscription.HasKey(s => s.Id);
        subscription.Property(s => s.Id)
            .HasConversion(new StronglyTypedIdConverter<SubscriptionId, Guid>(id => id.Value, value => new SubscriptionId(value)))
            .ValueGeneratedNever()
            .HasColumnName("id");
        subscription.Property(s => s.RanchId).HasColumnName("ranch_id");
        subscription.Property(s => s.PlanId).HasConversion(PlanIdConverter()).HasColumnName("plan_id");
        subscription.Property(s => s.HeadCount)
            .HasConversion(count => count.Value, value => new HeadCount(value))
            .HasColumnName("head_count");
        subscription.OwnsOne(s => s.AnnualFee, money => ConfigureMoney(money, "annual_fee"));
        subscription.OwnsOne(s => s.DevicesCost, money => ConfigureMoney(money, "devices_cost"));
        subscription.OwnsOne(s => s.Total, money => ConfigureMoney(money, "total"));
        subscription.Property(s => s.Status).HasConversion<string>().HasMaxLength(EnumLength).HasColumnName("status");
        subscription.Property(s => s.TermsVersion)
            .HasMaxLength(MaximumTermsVersionLength)
            .IsRequired()
            .HasColumnName("terms_version");
        subscription.Property(s => s.OperationId).HasColumnName("operation_id");
        subscription.Property(s => s.AcquisitionSource)
            .HasMaxLength(MaximumAcquisitionSourceLength)
            .HasColumnName("acquisition_source");
        subscription.Property(s => s.RequestedAt).HasColumnName("requested_at");
        subscription.Property(s => s.ActivatedAt).HasColumnName("activated_at");
        subscription.Property(s => s.ValidUntil).HasColumnName("valid_until");
        subscription.Property(s => s.CancelledAt).HasColumnName("cancelled_at");
        subscription.Property(s => s.CancellationReason)
            .HasMaxLength(Subscription.MaximumCancellationReasonLength)
            .HasColumnName("cancellation_reason");
        subscription.HasIndex(s => s.OperationId).IsUnique();
        subscription.HasIndex(s => new { s.RanchId, s.Status });

        // Concurrent requests for one ranch must not both create a current subscription, so the database enforces it.
        subscription.HasIndex(s => s.RanchId).IsUnique().HasFilter("status IN ('Pending', 'Active')");
    }

    private static void ConfigureMoney<TOwner>(OwnedNavigationBuilder<TOwner, Money> money, string columnPrefix)
        where TOwner : class
    {
        money.Property(m => m.Amount).HasPrecision(12, 2).HasColumnName($"{columnPrefix}_amount");
        money.Property(m => m.Currency).HasMaxLength(3).IsFixedLength().HasColumnName($"{columnPrefix}_currency");
    }

    private static StronglyTypedIdConverter<PlanId, Guid> PlanIdConverter() =>
        new(id => id.Value, value => new PlanId(value));

    private static object CatalogPlan(Guid id, PlanTier tier, string name, int deviceLimit) => new
    {
        Id = new PlanId(id),
        Tier = tier,
        Name = name,
        DeviceLimit = new DeviceLimit(deviceLimit),
        IsPublished = true
    };

    private static object MoneyRow(Guid planId, decimal amount) => new
    {
        PlanId = new PlanId(planId),
        Amount = amount,
        Currency
    };

    private static object FeatureRow(Guid planId, FeatureCode feature) => new
    {
        plan_id = new PlanId(planId),
        Code = feature
    };
}
