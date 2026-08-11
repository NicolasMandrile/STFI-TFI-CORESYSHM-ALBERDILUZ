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
        builder.Property(x => x.IdempotencyKey).HasMaxLength(100);
        // .IsRowVersion() se aplica condicionalmente en ApplicationDbContext.OnModelCreating
        // (solo SQL Server la soporta de forma nativa; rompe el esquema de los tests con Sqlite).

        builder.HasIndex(x => x.NumeroFactura).IsUnique();
        builder.HasIndex(x => x.IdempotencyKey)
               .IsUnique()
               .HasFilter("[IdempotencyKey] IS NOT NULL")
               .HasDatabaseName("IX_Facturas_IdempotencyKey");

        // Ya NO es 1 a 1: una Venta puede tener varias Facturas (facturación parcial).
        builder.HasOne(x => x.Venta)
               .WithMany(v => v.Facturas)
               .HasForeignKey(x => x.VentaId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Cliente)
               .WithMany(c => c.Facturas)
               .HasForeignKey(x => x.ClienteId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.TipoComprobante)
               .WithMany(t => t.Facturas)
               .HasForeignKey(x => x.TipoComprobanteId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PuntoVenta)
               .WithMany(p => p.Facturas)
               .HasForeignKey(x => x.PuntoVentaId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.VentaId).HasDatabaseName("IX_Facturas_VentaId");
        builder.HasIndex(x => x.FechaEmision).HasDatabaseName("IX_Facturas_FechaEmision");
        builder.HasIndex(x => x.PuntoVentaId).HasDatabaseName("IX_Facturas_PuntoVentaId");
        builder.HasIndex(x => x.TipoComprobanteId).HasDatabaseName("IX_Facturas_TipoComprobanteId");
    }
}
