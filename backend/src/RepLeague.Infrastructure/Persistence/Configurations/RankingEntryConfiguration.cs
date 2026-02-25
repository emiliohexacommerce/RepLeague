using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RepLeague.Domain.Entities;

namespace RepLeague.Infrastructure.Persistence.Configurations;

public class RankingEntryConfiguration : IEntityTypeConfiguration<RankingEntry>
{
    public void Configure(EntityTypeBuilder<RankingEntry> builder)
    {
        builder.HasKey(r => r.Id);
        builder.HasIndex(r => new { r.LeagueId, r.UserId }).IsUnique();
        builder.HasIndex(r => new { r.LeagueId, r.Points });

        builder.HasOne(r => r.User)
            .WithMany(u => u.RankingEntries)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
