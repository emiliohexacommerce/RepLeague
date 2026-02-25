using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RepLeague.Domain.Entities;

namespace RepLeague.Infrastructure.Persistence.Configurations;

public class LeagueConfiguration : IEntityTypeConfiguration<League>
{
    public void Configure(EntityTypeBuilder<League> builder)
    {
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Name).HasMaxLength(100).IsRequired();
        builder.Property(l => l.Description).HasMaxLength(500);
        builder.Property(l => l.ImageUrl).HasMaxLength(500);

        builder.HasOne(l => l.Owner)
            .WithMany(u => u.OwnedLeagues)
            .HasForeignKey(l => l.OwnerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(l => l.Members)
            .WithOne(m => m.League)
            .HasForeignKey(m => m.LeagueId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(l => l.Invitations)
            .WithOne(i => i.League)
            .HasForeignKey(i => i.LeagueId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(l => l.Rankings)
            .WithOne(r => r.League)
            .HasForeignKey(r => r.LeagueId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
