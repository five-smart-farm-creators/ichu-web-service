using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

/// <summary>
///     Entity Framework Core mapping of the <see cref="Geofence"/> aggregate. The shared database context applies it
///     through ApplyConfigurationsFromAssembly.
/// </summary>
public class GeofenceConfiguration : IEntityTypeConfiguration<Geofence>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Geofence> builder)
    {
        builder.ToTable("Geofences", MonitoringSchema.Name);
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Id).ValueGeneratedNever();
        builder.Property(g => g.Name).HasMaxLength(Geofence.MaximumNameLength).IsRequired();
        builder.Property(g => g.Severity).HasConversion<string>().HasMaxLength(MonitoringSchema.EnumLength);
        builder.HasIndex(g => new { g.RanchId, g.LotId }).IsUnique();

        // Vertices use a surrogate key because a redefinition replaces the whole boundary.
        builder.OwnsMany(g => g.Vertices, vertex =>
        {
            vertex.ToTable("GeofenceVertices", MonitoringSchema.Name);
            vertex.WithOwner().HasForeignKey("GeofenceId");
            vertex.Property<long>("Id").ValueGeneratedOnAdd();
            vertex.HasKey("Id");
            vertex.HasIndex("GeofenceId", nameof(GeofenceVertex.Sequence)).IsUnique();
        });
        builder.Navigation(g => g.Vertices).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
