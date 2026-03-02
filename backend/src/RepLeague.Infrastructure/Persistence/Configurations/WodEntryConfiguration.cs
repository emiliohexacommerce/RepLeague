using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RepLeague.Domain.Entities;

namespace RepLeague.Infrastructure.Persistence.Configurations;

public class WodEntryConfiguration : IEntityTypeConfiguration<WodEntry>
{
    public void Configure(EntityTypeBuilder<WodEntry> builder)
    {
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Type).HasMaxLength(16).IsRequired();
        builder.Property(w => w.Title).HasMaxLength(100);
        builder.Property(w => w.Notes).HasMaxLength(1000);

        builder.HasOne(w => w.User)
            .WithMany()
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(w => w.Exercises)
            .WithOne(e => e.WodEntry)
            .HasForeignKey(e => e.WodEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(w => w.AmrapResult)
            .WithOne(r => r.WodEntry)
            .HasForeignKey<WodResultAmrap>(r => r.WodEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(w => w.EmomResult)
            .WithOne(r => r.WodEntry)
            .HasForeignKey<WodResultEmom>(r => r.WodEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(w => new { w.UserId, w.Date })
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(w => new { w.Type, w.Date })
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(w => new { w.UserId, w.Title })
            .HasFilter("[IsDeleted] = 0 AND [Title] IS NOT NULL");
    }
}
