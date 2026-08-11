using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoreSysHM.Domain.Entities.Facturacion;

namespace CoreSysHM.Infrastructure.Data.Configurations.Facturacion;

public class TipoComprobanteConfiguration : IEntityTypeConfiguration<TipoComprobante>
{
    public void Configure(EntityTypeBuilder<TipoComprobante> builder)
    {
        builder.ToTable("TiposComprobante");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Descripcion).IsRequired().HasMaxLength(50);
        builder.Property(x => x.SignoContable).IsRequired().HasMaxLength(1);
        builder.HasIndex(x => x.Descripcion).IsUnique();
    }
}
