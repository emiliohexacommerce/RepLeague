using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RepLeague.Domain.Entities;

namespace RepLeague.Infrastructure.Persistence.Configurations;

public class DailyPointsConfiguration : IEntityTypeConfiguration<DailyPoints>
{
    public void Configure(EntityTypeBuilder<DailyPoints> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Ignore(p => p.TotalPoints);

        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.League)
            .WithMany(l => l.DailyPoints)
            .HasForeignKey(p => p.LeagueId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => new { p.UserId, p.LeagueId, p.Date }).IsUnique();
    }
}
