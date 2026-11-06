using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Infrastructure.Persistence.EntityFrameworkCore;

/// <summary>
/// EF Core persistence context for Planning-owned records.
/// </summary>
public sealed class PlanningDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlanningDbContext"/> class.
    /// </summary>
    /// <param name="options">The context options.</param>
    public PlanningDbContext(DbContextOptions<PlanningDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets Planning campaigns.
    /// </summary>
    public DbSet<PlanningCampaign> Campaigns => Set<PlanningCampaign>();

    /// <summary>
    /// Gets planned activities.
    /// </summary>
    public DbSet<PlannedActivity> Activities => Set<PlannedActivity>();

    /// <summary>
    /// Gets application records.
    /// </summary>
    public DbSet<ApplicationRecord> Applications => Set<ApplicationRecord>();

    /// <summary>
    /// Gets follow-ups.
    /// </summary>
    public DbSet<FollowUp> FollowUps => Set<FollowUp>();

    /// <summary>
    /// Gets withdrawal periods.
    /// </summary>
    public DbSet<WithdrawalPeriod> WithdrawalPeriods => Set<WithdrawalPeriod>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PlanningDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
