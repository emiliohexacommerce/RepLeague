using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RepLeague.Domain.Entities;

namespace RepLeague.Infrastructure.Persistence.Configurations;

public class InvitationConfiguration : IEntityTypeConfiguration<Invitation>
{
    public void Configure(EntityTypeBuilder<Invitation> builder)
    {
        builder.HasKey(i => i.Id);
        builder.HasIndex(i => i.Token).IsUnique();
        builder.Property(i => i.Token).HasMaxLength(200).IsRequired();
        builder.Property(i => i.Email).HasMaxLength(256);
        builder.Property(i => i.Status).IsRequired();
    }
}
