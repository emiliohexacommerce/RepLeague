using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RepLeague.Domain.Entities;

namespace RepLeague.Infrastructure.Persistence.Configurations;

public class DailyWodConfiguration : IEntityTypeConfiguration<DailyWod>
{
    public void Configure(EntityTypeBuilder<DailyWod> builder)
    {
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Type).HasMaxLength(20).IsRequired();
        builder.Property(w => w.Title).HasMaxLength(200).IsRequired();
        builder.Property(w => w.Notes).HasMaxLength(1000);

        builder.HasOne(w => w.League)
            .WithMany(l => l.DailyWods)
            .HasForeignKey(w => w.LeagueId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(w => w.SetByUser)
            .WithMany()
            .HasForeignKey(w => w.SetByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(w => w.Exercises)
            .WithOne(e => e.DailyWod)
            .HasForeignKey(e => e.DailyWodId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(w => w.Results)
            .WithOne(r => r.DailyWod)
            .HasForeignKey(r => r.DailyWodId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(w => new { w.LeagueId, w.Date }).IsUnique();
    }
}
