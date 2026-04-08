using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RepLeague.Domain.Entities;

namespace RepLeague.Infrastructure.Persistence.Configurations;

public class DailyWodResultConfiguration : IEntityTypeConfiguration<DailyWodResult>
{
    public void Configure(EntityTypeBuilder<DailyWodResult> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Notes).HasMaxLength(1000);

        builder.HasOne(r => r.DailyWod)
            .WithMany(w => w.Results)
            .HasForeignKey(r => r.DailyWodId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.ExerciseDetails)
            .WithOne(e => e.DailyWodResult)
            .HasForeignKey(e => e.DailyWodResultId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => new { r.DailyWodId, r.UserId }).IsUnique();
    }
}
