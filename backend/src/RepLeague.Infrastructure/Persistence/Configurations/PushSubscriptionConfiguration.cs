using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RepLeague.Domain.Entities;

namespace RepLeague.Infrastructure.Persistence.Configurations;

public class PushSubscriptionConfiguration : IEntityTypeConfiguration<PushSubscription>
{
    public void Configure(EntityTypeBuilder<PushSubscription> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedOnAdd();

        builder.Property(p => p.Endpoint).IsRequired().HasMaxLength(500);
        builder.Property(p => p.P256dh).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Auth).IsRequired().HasMaxLength(100);

        builder.HasOne(p => p.User)
               .WithMany()
               .HasForeignKey(p => p.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        // Unique index on endpoint (each device has one subscription)
        builder.HasIndex(p => p.Endpoint).IsUnique();
    }
}
