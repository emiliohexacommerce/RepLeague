using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RepLeague.Domain.Entities;

namespace RepLeague.Infrastructure.Persistence.Configurations;

public class WorkoutExerciseConfiguration : IEntityTypeConfiguration<WorkoutExercise>
{
    public void Configure(EntityTypeBuilder<WorkoutExercise> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ExerciseName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.WeightKg).HasPrecision(7, 2);
    }
}
