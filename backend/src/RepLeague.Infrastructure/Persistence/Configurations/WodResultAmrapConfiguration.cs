using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RepLeague.Domain.Entities;

namespace RepLeague.Infrastructure.Persistence.Configurations;

public class WodResultAmrapConfiguration : IEntityTypeConfiguration<WodResultAmrap>
{
    public void Configure(EntityTypeBuilder<WodResultAmrap> builder)
    {
        builder.HasKey(r => r.WodEntryId);
    }
}
