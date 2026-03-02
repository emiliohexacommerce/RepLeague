using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RepLeague.Domain.Entities;

namespace RepLeague.Infrastructure.Persistence.Configurations;

public class WodExerciseConfiguration : IEntityTypeConfiguration<WodExercise>
{
    public void Configure(EntityTypeBuilder<WodExercise> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name).HasMaxLength(80).IsRequired();
        builder.Property(e => e.MovementType).HasMaxLength(24).IsRequired();
        builder.Property(e => e.LoadUnit).HasMaxLength(8);
        builder.Property(e => e.Notes).HasMaxLength(400);
        builder.Property(e => e.LoadValue).HasPrecision(6, 2);

        builder.HasIndex(e => new { e.WodEntryId, e.OrderIndex }).IsUnique();
    }
}
