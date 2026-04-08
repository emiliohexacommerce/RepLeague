using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RepLeague.Domain.Entities;

namespace RepLeague.Infrastructure.Persistence.Configurations;

public class DailyWodResultExerciseConfiguration : IEntityTypeConfiguration<DailyWodResultExercise>
{
    public void Configure(EntityTypeBuilder<DailyWodResultExercise> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.WeightUsedKg).HasColumnType("decimal(6,2)");
        builder.Property(e => e.Notes).HasMaxLength(500);

        builder.HasOne(e => e.DailyWodResult)
            .WithMany(r => r.ExerciseDetails)
            .HasForeignKey(e => e.DailyWodResultId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.DailyWodExercise)
            .WithMany()
            .HasForeignKey(e => e.DailyWodExerciseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
