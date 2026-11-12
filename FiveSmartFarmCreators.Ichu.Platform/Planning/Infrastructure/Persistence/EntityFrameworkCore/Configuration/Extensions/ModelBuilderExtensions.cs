using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;

/// <summary>
///     Entity Framework Core mappings of the Planning context. The database also enforces the campaign, withdrawal and nutrition date ranges, and application records, follow-ups and ration items are owned by their parent.
/// </summary>
public static class ModelBuilderExtensions
{
    private const string Schema = "planning";
    private const int CampaignKindLength = 20;
    private const int RationKindLength = 20;

    /// <summary>
    ///     Maps campaigns, activities with their records, withdrawal periods and nutrition plans under the planning schema.
    /// </summary>
    public static void ApplyPlanningConfiguration(this ModelBuilder builder)
    {
        ConfigurePlanningCampaign(builder.Entity<PlanningCampaign>());
        ConfigurePlannedActivity(builder.Entity<PlannedActivity>());
        ConfigureApplicationRecord(builder.Entity<ApplicationRecord>());
        ConfigureFollowUp(builder.Entity<FollowUp>());
        ConfigureWithdrawalPeriod(builder.Entity<WithdrawalPeriod>());
        ConfigureNutritionPlan(builder.Entity<NutritionPlan>());
    }

    private static void ConfigureNutritionPlan(EntityTypeBuilder<NutritionPlan> plan)
    {
        plan.ToTable("nutrition_plans", Schema, table =>
            table.HasCheckConstraint("ck_nutrition_plans_validity", "valid_to IS NULL OR valid_to >= valid_from"));
        plan.HasKey(p => p.Id);
        plan.Property(p => p.Id).HasConversion(NutritionPlanIdConverter()).HasColumnName("id").ValueGeneratedNever();
        plan.Property(p => p.RanchId).HasColumnName("ranch_id").IsRequired();
        plan.Property(p => p.LotId).HasColumnName("lot_id").IsRequired();
        plan.Property(p => p.Name).HasColumnName("name").HasMaxLength(NutritionPlan.MaximumNameLength).IsRequired();
        plan.Property(p => p.ValidFrom).HasColumnName("valid_from").HasColumnType("date").IsRequired();
        plan.Property(p => p.ValidTo).HasColumnName("valid_to").HasColumnType("date");
        plan.Property(p => p.TargetDailyGainKilograms).HasColumnName("target_daily_gain_kilograms").HasColumnType("numeric(5,3)");
        plan.Property(p => p.Notes).HasColumnName("notes").HasMaxLength(NutritionPlan.MaximumNotesLength);
        plan.HasIndex(p => new { p.LotId, p.ValidFrom }).HasDatabaseName("ix_nutrition_plans_lot_id_valid_from");
        plan.OwnsMany(p => p.Items, item =>
        {
            item.ToTable("nutrition_plan_items", Schema);
            item.Property<NutritionPlanId>("NutritionPlanId").HasConversion(NutritionPlanIdConverter()).HasColumnName("nutrition_plan_id");
            item.WithOwner().HasForeignKey("NutritionPlanId");
            item.Property<long>("Id").HasColumnName("id").ValueGeneratedOnAdd();
            item.HasKey("Id");
            item.Property(i => i.Feed).HasColumnName("feed").HasMaxLength(RationItem.MaximumFeedLength).IsRequired();
            item.Property(i => i.Kind).HasColumnName("kind").HasConversion<string>().HasMaxLength(RationKindLength).IsRequired();
            item.Property(i => i.KilogramsPerHeadPerDay).HasColumnName("kilograms_per_head_per_day").HasColumnType("numeric(7,3)");
        });
        plan.Navigation(p => p.Items).HasField("_items").UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigurePlanningCampaign(EntityTypeBuilder<PlanningCampaign> campaign)
    {
        campaign.ToTable("campaigns", Schema, table =>
            table.HasCheckConstraint("ck_campaigns_date_range", "ends_on >= starts_on"));
        campaign.HasKey(c => c.Id);
        campaign.Property(c => c.Id).HasConversion(PlanningCampaignIdConverter()).HasColumnName("id").ValueGeneratedNever();
        campaign.Property(c => c.RanchId).HasColumnName("ranch_id").IsRequired();
        campaign.Property(c => c.Name).HasColumnName("name").IsRequired();
        campaign.Property(c => c.Kind).HasColumnName("kind").HasConversion<string>().HasMaxLength(CampaignKindLength).IsRequired();
        campaign.Property(c => c.StartsOn).HasColumnName("starts_on").HasColumnType("date").IsRequired();
        campaign.Property(c => c.EndsOn).HasColumnName("ends_on").HasColumnType("date").IsRequired();
        campaign.HasIndex(c => new { c.RanchId, c.StartsOn }).HasDatabaseName("ix_campaigns_ranch_id_starts_on");
    }

    private static void ConfigurePlannedActivity(EntityTypeBuilder<PlannedActivity> activity)
    {
        activity.ToTable("planned_activities", Schema);
        activity.HasKey(a => a.Id);
        activity.Property(a => a.Id).HasConversion(PlannedActivityIdConverter()).HasColumnName("id").ValueGeneratedNever();
        activity.Property(a => a.RanchId).HasColumnName("ranch_id").IsRequired();
        activity.Property(a => a.CampaignId).HasConversion(NullablePlanningCampaignIdConverter()).HasColumnName("campaign_id");
        activity.Property(a => a.CattleId).HasColumnName("cattle_id");
        activity.Property(a => a.Title).HasColumnName("title").IsRequired();
        activity.Property(a => a.ScheduledAt)
            .HasColumnName("scheduled_at").HasColumnType("timestamp with time zone").IsRequired();
        activity.HasOne<PlanningCampaign>()
            .WithMany()
            .HasForeignKey(a => a.CampaignId)
            .OnDelete(DeleteBehavior.Restrict);
        activity.HasMany(a => a.Applications)
            .WithOne()
            .HasForeignKey(application => application.ActivityId)
            .OnDelete(DeleteBehavior.Cascade);
        activity.HasMany(a => a.FollowUps)
            .WithOne()
            .HasForeignKey(followUp => followUp.ActivityId)
            .OnDelete(DeleteBehavior.Cascade);
        activity.Navigation(a => a.Applications).HasField("_applications").UsePropertyAccessMode(PropertyAccessMode.Field);
        activity.Navigation(a => a.FollowUps).HasField("_followUps").UsePropertyAccessMode(PropertyAccessMode.Field);
        activity.HasIndex(a => new { a.RanchId, a.ScheduledAt }).HasDatabaseName("ix_planned_activities_ranch_id_scheduled_at");
        activity.HasIndex(a => a.CampaignId).HasDatabaseName("ix_planned_activities_campaign_id");
    }

    private static void ConfigureApplicationRecord(EntityTypeBuilder<ApplicationRecord> application)
    {
        application.ToTable("application_records", Schema);
        application.HasKey(a => a.Id);
        application.Property(a => a.Id).HasConversion(ApplicationRecordIdConverter()).HasColumnName("id").ValueGeneratedNever();
        application.Property(a => a.ActivityId).HasConversion(PlannedActivityIdConverter()).HasColumnName("activity_id").IsRequired();
        application.Property(a => a.CattleId).HasColumnName("cattle_id").IsRequired();
        application.Property(a => a.AppliedAt)
            .HasColumnName("applied_at").HasColumnType("timestamp with time zone").IsRequired();
        application.Property(a => a.InterventionId).HasColumnName("intervention_id");
        application.HasIndex(a => new { a.ActivityId, a.AppliedAt }).HasDatabaseName("ix_application_records_activity_id_applied_at");
        application.HasIndex(a => a.InterventionId).HasDatabaseName("ix_application_records_intervention_id");
    }

    private static void ConfigureFollowUp(EntityTypeBuilder<FollowUp> followUp)
    {
        followUp.ToTable("follow_ups", Schema);
        followUp.HasKey(f => f.Id);
        followUp.Property(f => f.Id).HasConversion(FollowUpIdConverter()).HasColumnName("id").ValueGeneratedNever();
        followUp.Property(f => f.ActivityId).HasConversion(PlannedActivityIdConverter()).HasColumnName("activity_id").IsRequired();
        followUp.Property(f => f.DueAt).HasColumnName("due_at").HasColumnType("timestamp with time zone").IsRequired();
        followUp.HasIndex(f => new { f.ActivityId, f.DueAt }).HasDatabaseName("ix_follow_ups_activity_id_due_at");
    }

    private static void ConfigureWithdrawalPeriod(EntityTypeBuilder<WithdrawalPeriod> period)
    {
        period.ToTable("withdrawal_periods", Schema, table =>
            table.HasCheckConstraint("ck_withdrawal_periods_interval", "ends_at >= starts_at"));
        period.HasKey(p => p.Id);
        period.Property(p => p.Id).HasConversion(WithdrawalPeriodIdConverter()).HasColumnName("id").ValueGeneratedNever();
        period.Property(p => p.RanchId).HasColumnName("ranch_id").IsRequired();
        period.Property(p => p.CattleId).HasColumnName("cattle_id").IsRequired();
        period.Property(p => p.InterventionId).HasColumnName("intervention_id");
        period.Property(p => p.StartsAt).HasColumnName("starts_at").HasColumnType("timestamp with time zone").IsRequired();
        period.Property(p => p.EndsAt).HasColumnName("ends_at").HasColumnType("timestamp with time zone").IsRequired();
        period.HasIndex(p => new { p.RanchId, p.CattleId, p.StartsAt }).HasDatabaseName("ix_withdrawal_periods_ranch_cattle_starts_at");
        period.HasIndex(p => p.InterventionId).HasDatabaseName("ix_withdrawal_periods_intervention_id");
    }

    private static StronglyTypedIdConverter<PlanningCampaignId, Guid> PlanningCampaignIdConverter() =>
        new(id => id.Value, value => new PlanningCampaignId(value));

    private static StronglyTypedIdConverter<PlannedActivityId, Guid> PlannedActivityIdConverter() =>
        new(id => id.Value, value => new PlannedActivityId(value));

    private static StronglyTypedIdConverter<ApplicationRecordId, Guid> ApplicationRecordIdConverter() =>
        new(id => id.Value, value => new ApplicationRecordId(value));

    private static StronglyTypedIdConverter<FollowUpId, Guid> FollowUpIdConverter() =>
        new(id => id.Value, value => new FollowUpId(value));

    private static StronglyTypedIdConverter<WithdrawalPeriodId, Guid> WithdrawalPeriodIdConverter() =>
        new(id => id.Value, value => new WithdrawalPeriodId(value));

    private static StronglyTypedIdConverter<NutritionPlanId, Guid> NutritionPlanIdConverter() =>
        new(id => id.Value, value => new NutritionPlanId(value));

    private static ValueConverter<PlanningCampaignId?, Guid?> NullablePlanningCampaignIdConverter() =>
        new(id => id.HasValue ? id.Value.Value : (Guid?)null, value => value.HasValue ? new PlanningCampaignId(value.Value) : (PlanningCampaignId?)null);
}
