using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RepLeague.Domain.Entities;

namespace RepLeague.Infrastructure.Persistence.Configurations;

public class WodResultEmomConfiguration : IEntityTypeConfiguration<WodResultEmom>
{
    public void Configure(EntityTypeBuilder<WodResultEmom> builder)
    {
        builder.HasKey(r => r.WodEntryId);
    }
}
