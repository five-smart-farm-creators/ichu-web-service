using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;

/// <summary>
///     Entity Framework Core mappings of the Identity and Access context. They keep the table, column and index names of the
///     iam schema, and the typed identifiers are stored as their Guid values.
/// </summary>
public static class ModelBuilderExtensions
{
    private const string Schema = "iam";

    /// <summary>
    ///     Maps accounts with their professional profiles, ranches, memberships, advisories and access attempts. The shared database context calls it from OnModelCreating.
    /// </summary>
    public static void ApplyIamConfiguration(this ModelBuilder builder)
    {
        ConfigureUserAccount(builder.Entity<UserAccount>());
        ConfigureRanchRegistration(builder.Entity<RanchRegistration>());
        ConfigureRanchMembership(builder.Entity<RanchMembership>());
        ConfigureHerdAdvisory(builder.Entity<HerdAdvisory>());
        ConfigureAccessAttempt(builder.Entity<AccessAttempt>());
    }

    private static void ConfigureUserAccount(EntityTypeBuilder<UserAccount> account)
    {
        account.ToTable("UserAccounts", Schema);
        account.HasKey(a => a.Id);
        account.Property(a => a.Id).HasConversion(UserAccountIdConverter()).ValueGeneratedNever();
        account.Property(a => a.ExternalUid).HasMaxLength(128).IsRequired();
        account.HasIndex(a => a.ExternalUid).IsUnique();
        account.Property(a => a.Email).HasConversion(email => email.Value, value => new Email(value)).HasMaxLength(254).IsRequired();
        account.HasIndex(a => a.Email).IsUnique();
        account.Property(a => a.Role).HasConversion<string>().HasMaxLength(32);
        account.Property(a => a.Status).HasConversion<string>().HasMaxLength(32);
        account.Property(a => a.Revision).IsConcurrencyToken();
        account.OwnsOne(a => a.Profile, profile =>
        {
            profile.ToTable("ProfessionalProfiles", Schema);
            profile.Property<UserAccountId>("UserAccountId").HasConversion(UserAccountIdConverter());
            profile.Property(p => p.Specialty).HasMaxLength(120).IsRequired();
            profile.Property(p => p.LicenseNumber).HasMaxLength(80).IsRequired();
            profile.HasIndex(p => p.LicenseNumber).IsUnique();
        });
    }

    private static void ConfigureRanchRegistration(EntityTypeBuilder<RanchRegistration> ranch)
    {
        ranch.ToTable("RanchRegistrations", Schema);
        ranch.HasKey(r => r.Id);
        ranch.Property(r => r.Name).HasMaxLength(160).IsRequired();
        ranch.Property(r => r.Region).HasMaxLength(120).IsRequired();
        ranch.Property(r => r.District).HasMaxLength(120).IsRequired();
        ranch.Property(r => r.AccessCode).HasConversion(code => code.Value, value => new RanchAccessCode(value))
            .HasMaxLength(RanchAccessCode.Length).IsRequired();
        ranch.HasIndex(r => r.AccessCode).IsUnique().HasDatabaseName("IX_RanchRegistrations_AccessCode");
    }

    private static void ConfigureRanchMembership(EntityTypeBuilder<RanchMembership> membership)
    {
        membership.ToTable("RanchMemberships", Schema);
        membership.HasKey(m => m.Id);
        membership.Property(m => m.UserAccountId).HasConversion(UserAccountIdConverter());
        membership.HasIndex(m => new { m.UserAccountId, m.RanchId }).IsUnique();
        membership.HasOne<UserAccount>().WithMany().HasForeignKey(m => m.UserAccountId).OnDelete(DeleteBehavior.Restrict);
        membership.HasOne<RanchRegistration>().WithMany().HasForeignKey(m => m.RanchId).OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureHerdAdvisory(EntityTypeBuilder<HerdAdvisory> advisory)
    {
        advisory.ToTable("HerdAdvisories", Schema);
        advisory.HasKey(a => a.Id);
        advisory.Property(a => a.Id).HasConversion(HerdAdvisoryIdConverter()).ValueGeneratedNever();
        advisory.Property(a => a.VeterinarianId).HasConversion(UserAccountIdConverter());
        advisory.Property(a => a.DecidedBy).HasConversion(NullableUserAccountIdConverter());
        advisory.Property(a => a.Reason).HasMaxLength(1000).IsRequired();
        advisory.Property(a => a.Status).HasConversion<string>().HasMaxLength(32);
        advisory.Property(a => a.Scope).HasConversion<string>().HasMaxLength(32);
        advisory.Property(a => a.Revision).IsConcurrencyToken();
        advisory.HasIndex(a => new { a.VeterinarianId, a.RanchId }).IsUnique()
            .HasFilter("\"Status\" IN ('Pending', 'Granted')");
        advisory.HasOne<UserAccount>().WithMany().HasForeignKey(a => a.VeterinarianId).OnDelete(DeleteBehavior.Restrict);
        advisory.HasOne<RanchRegistration>().WithMany().HasForeignKey(a => a.RanchId).OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureAccessAttempt(EntityTypeBuilder<AccessAttempt> attempt)
    {
        attempt.ToTable("AccessAttempts", Schema);
        attempt.HasKey(a => a.Id);
        attempt.Property(a => a.UserAccountId).HasConversion(UserAccountIdConverter());
        attempt.HasIndex(a => new { a.UserAccountId, a.RecordedAt });
        attempt.HasOne<UserAccount>().WithMany().HasForeignKey(a => a.UserAccountId).OnDelete(DeleteBehavior.Restrict);
    }

    private static StronglyTypedIdConverter<UserAccountId, Guid> UserAccountIdConverter() =>
        new(id => id.Value, value => new UserAccountId(value));

    private static StronglyTypedIdConverter<HerdAdvisoryId, Guid> HerdAdvisoryIdConverter() =>
        new(id => id.Value, value => new HerdAdvisoryId(value));

    private static ValueConverter<UserAccountId?, Guid?> NullableUserAccountIdConverter() =>
        new(id => id.HasValue ? id.Value.Value : (Guid?)null,
            value => value.HasValue ? new UserAccountId(value.Value) : (UserAccountId?)null);
}
