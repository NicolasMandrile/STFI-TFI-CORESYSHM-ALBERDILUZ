using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoreSysHM.Domain.Entities.Facturacion;

namespace CoreSysHM.Infrastructure.Data.Configurations.Facturacion;

public class NumeracionComprobanteConfiguration : IEntityTypeConfiguration<NumeracionComprobante>
{
    public void Configure(EntityTypeBuilder<NumeracionComprobante> builder)
    {
        builder.ToTable("NumeracionesComprobante");
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.PuntoVenta)
               .WithMany()
               .HasForeignKey(x => x.PuntoVentaId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.TipoComprobante)
               .WithMany()
               .HasForeignKey(x => x.TipoComprobanteId)
               .OnDelete(DeleteBehavior.Restrict);

        // Un único contador por combinación punto de venta + tipo de comprobante.
        builder.HasIndex(x => new { x.PuntoVentaId, x.TipoComprobanteId })
               .IsUnique()
               .HasDatabaseName("IX_NumeracionesComprobante_PuntoVenta_Tipo");
    }
}
