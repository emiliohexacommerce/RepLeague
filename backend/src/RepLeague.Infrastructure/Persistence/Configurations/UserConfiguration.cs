using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RepLeague.Domain.Entities;

namespace RepLeague.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.DisplayName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.PasswordHash).IsRequired();
        builder.Property(u => u.AvatarUrl).HasMaxLength(500);
        builder.Property(u => u.Country).HasMaxLength(3);
        builder.Property(u => u.Bio).HasMaxLength(500);

        // Extended profile fields
        builder.Property(u => u.Phone).HasMaxLength(20);
        builder.Property(u => u.City).HasMaxLength(100);
        builder.Property(u => u.GymName).HasMaxLength(80);
        builder.Property(u => u.Units).HasMaxLength(2).HasDefaultValue("kg");
        builder.Property(u => u.OneRmMethod).HasMaxLength(8).HasDefaultValue("Epley");
        builder.Property(u => u.Visibility).HasMaxLength(8).HasDefaultValue("leagues");
    }
}
