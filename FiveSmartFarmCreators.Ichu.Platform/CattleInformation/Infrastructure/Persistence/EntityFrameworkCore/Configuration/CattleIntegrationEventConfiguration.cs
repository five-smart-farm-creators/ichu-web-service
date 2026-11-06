using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

public sealed class CattleIntegrationEventConfiguration : IEntityTypeConfiguration<CattleIntegrationEvent>
{
    public void Configure(EntityTypeBuilder<CattleIntegrationEvent> builder)
    {
        builder.ToTable("integration_events", "cattle");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(item => item.EventType).HasColumnName("event_type").HasMaxLength(100).IsRequired();
        builder.Property(item => item.OccurredAt).HasColumnName("occurred_at").HasColumnType("timestamp with time zone");
        builder.Property(item => item.Payload).HasColumnName("payload").HasColumnType("jsonb").IsRequired();
        builder.Property(item => item.PublishedAt).HasColumnName("published_at").HasColumnType("timestamp with time zone");
        builder.HasIndex(item => new { item.PublishedAt, item.OccurredAt });
    }
}
