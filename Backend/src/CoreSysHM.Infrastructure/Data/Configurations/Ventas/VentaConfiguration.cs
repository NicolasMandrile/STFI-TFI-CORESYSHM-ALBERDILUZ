using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoreSysHM.Domain.Entities.Ventas;

namespace CoreSysHM.Infrastructure.Data.Configurations.Ventas;

public class VentaConfiguration : IEntityTypeConfiguration<Venta>
{
    public void Configure(EntityTypeBuilder<Venta> builder)
    {
        builder.ToTable("Ventas");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.NumeroVenta).IsRequired().HasMaxLength(20);
        builder.Property(x => x.Subtotal).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Descuento).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Total).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Estado).HasConversion<string>();
        builder.HasIndex(x => x.NumeroVenta).IsUnique();
        builder.HasOne(x => x.Cliente)
               .WithMany(c => c.Ventas)
               .HasForeignKey(x => x.ClienteId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
