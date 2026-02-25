using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RepLeague.Domain.Entities;

namespace RepLeague.Infrastructure.Persistence.Configurations;

public class LeagueMemberConfiguration : IEntityTypeConfiguration<LeagueMember>
{
    public void Configure(EntityTypeBuilder<LeagueMember> builder)
    {
        builder.HasKey(m => m.Id);
        builder.HasIndex(m => new { m.LeagueId, m.UserId }).IsUnique();

        builder.HasOne(m => m.User)
            .WithMany(u => u.LeagueMemberships)
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
