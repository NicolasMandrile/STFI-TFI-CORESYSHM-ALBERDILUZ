using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoreSysHM.Domain.Entities.Facturacion;

namespace CoreSysHM.Infrastructure.Data.Configurations.Facturacion;

public class FacturaConfiguration : IEntityTypeConfiguration<Factura>
{
    public void Configure(EntityTypeBuilder<Factura> builder)
    {
        builder.ToTable("Facturas");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.NumeroFactura).IsRequired().HasMaxLength(20);
        builder.Property(x => x.Subtotal).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Iva).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Total).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Estado).HasConversion<string>();
        builder.HasIndex(x => x.NumeroFactura).IsUnique();
        builder.HasOne(x => x.Venta)
               .WithOne(v => v.Factura)
               .HasForeignKey<Factura>(x => x.VentaId)
               .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Cliente)
               .WithMany(c => c.Facturas)
               .HasForeignKey(x => x.ClienteId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
