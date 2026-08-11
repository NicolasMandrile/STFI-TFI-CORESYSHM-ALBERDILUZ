using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoreSysHM.Domain.Entities.Facturacion;

namespace CoreSysHM.Infrastructure.Data.Configurations.Facturacion;

public class DetalleFacturaConfiguration : IEntityTypeConfiguration<DetalleFactura>
{
    public void Configure(EntityTypeBuilder<DetalleFactura> builder)
    {
        builder.ToTable("DetallesFactura");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PrecioUnitario).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Impuesto).HasColumnType("decimal(5,2)");
        builder.Property(x => x.Descuento).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Subtotal).HasColumnType("decimal(18,2)");

        builder.HasOne(x => x.Factura)
               .WithMany(f => f.Detalles)
               .HasForeignKey(x => x.FacturaId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Producto)
               .WithMany()
               .HasForeignKey(x => x.ProductoId)
               .OnDelete(DeleteBehavior.Restrict);

        // Restrict (no Cascade): DetalleVenta pertenece a un agregado distinto (Venta); nunca
        // debería borrarse en cascada desde una Factura.
        builder.HasOne(x => x.DetalleVenta)
               .WithMany()
               .HasForeignKey(x => x.DetalleVentaId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.DetalleVentaId).HasDatabaseName("IX_DetallesFactura_DetalleVentaId");
        builder.HasIndex(x => x.ProductoId).HasDatabaseName("IX_DetallesFactura_ProductoId");
    }
}
