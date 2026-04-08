using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RepLeague.Domain.Entities;

namespace RepLeague.Infrastructure.Persistence.Configurations;

public class DailyWodExerciseConfiguration : IEntityTypeConfiguration<DailyWodExercise>
{
    public void Configure(EntityTypeBuilder<DailyWodExercise> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ExerciseName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Notes).HasMaxLength(500);
        builder.Property(e => e.WeightKg).HasColumnType("decimal(6,2)");
        builder.Property(e => e.Order).IsRequired();

        builder.HasOne(e => e.DailyWod)
            .WithMany(w => w.Exercises)
            .HasForeignKey(e => e.DailyWodId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
