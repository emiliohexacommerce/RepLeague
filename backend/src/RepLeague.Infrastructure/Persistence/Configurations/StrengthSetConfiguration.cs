using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RepLeague.Domain.Entities;

namespace RepLeague.Infrastructure.Persistence.Configurations;

public class StrengthSetConfiguration : IEntityTypeConfiguration<StrengthSet>
{
    public void Configure(EntityTypeBuilder<StrengthSet> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ExerciseName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.WeightKg).HasColumnType("decimal(6,2)");
        builder.Property(x => x.OneRepMaxKg).HasColumnType("decimal(6,2)");
        builder.Property(x => x.Notes).HasMaxLength(500);

        builder.HasIndex(x => new { x.LiftSessionId, x.ExerciseName, x.SetNumber });
    }
}
