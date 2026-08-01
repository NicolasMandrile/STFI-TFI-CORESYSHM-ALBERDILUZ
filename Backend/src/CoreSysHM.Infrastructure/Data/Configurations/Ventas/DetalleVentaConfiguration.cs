using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoreSysHM.Domain.Entities.Ventas;

namespace CoreSysHM.Infrastructure.Data.Configurations.Ventas;

public class DetalleVentaConfiguration : IEntityTypeConfiguration<DetalleVenta>
{
    public void Configure(EntityTypeBuilder<DetalleVenta> builder)
    {
        builder.ToTable("DetallesVenta");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PrecioUnitario).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Subtotal).HasColumnType("decimal(18,2)");
        builder.HasOne(x => x.Venta)
               .WithMany(v => v.Detalles)
               .HasForeignKey(x => x.VentaId)
               .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Producto)
               .WithMany()
               .HasForeignKey(x => x.ProductoId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
