using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RepLeague.Domain.Entities;

namespace RepLeague.Infrastructure.Persistence.Configurations;

public class ManualLiftPrConfiguration : IEntityTypeConfiguration<ManualLiftPr>
{
    public void Configure(EntityTypeBuilder<ManualLiftPr> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ExerciseName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.WeightKg).HasColumnType("decimal(6,2)");
        builder.Property(x => x.Notes).HasMaxLength(500);

        builder.HasIndex(x => new { x.UserId, x.ExerciseName })
               .HasFilter("[IsDeleted] = 0");

        builder.HasOne(x => x.User)
               .WithMany()
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
