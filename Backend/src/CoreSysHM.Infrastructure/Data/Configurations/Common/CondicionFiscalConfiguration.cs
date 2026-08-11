using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoreSysHM.Domain.Entities.Common;

namespace CoreSysHM.Infrastructure.Data.Configurations.Common;

public class CondicionFiscalConfiguration : IEntityTypeConfiguration<CondicionFiscal>
{
    public void Configure(EntityTypeBuilder<CondicionFiscal> builder)
    {
        builder.ToTable("CondicionesFiscales");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Descripcion).IsRequired().HasMaxLength(50);
        builder.HasIndex(x => x.Descripcion).IsUnique();
    }
}
