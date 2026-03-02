using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RepLeague.Domain.Entities;

namespace RepLeague.Infrastructure.Persistence.Configurations;

public class LiftSessionConfiguration : IEntityTypeConfiguration<LiftSession>
{
    public void Configure(EntityTypeBuilder<LiftSession> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Title).HasMaxLength(150);
        builder.Property(s => s.Notes).HasMaxLength(2000);

        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.Sets)
            .WithOne(x => x.LiftSession)
            .HasForeignKey(x => x.LiftSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => new { s.UserId, s.Date })
            .HasFilter("[IsDeleted] = 0");

        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}
