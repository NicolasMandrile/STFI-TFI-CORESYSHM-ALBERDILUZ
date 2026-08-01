using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoreSysHM.Domain.Entities.Stock;

namespace CoreSysHM.Infrastructure.Data.Configurations.Stock;

public class ProductoConfiguration : IEntityTypeConfiguration<Producto>
{
    public void Configure(EntityTypeBuilder<Producto> builder)
    {
        builder.ToTable("Productos");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Codigo).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Nombre).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Descripcion).HasMaxLength(500);
        builder.Property(x => x.PrecioCompra).HasColumnType("decimal(18,2)");
        builder.Property(x => x.PrecioVenta).HasColumnType("decimal(18,2)");
        builder.HasIndex(x => x.Codigo).IsUnique();
        builder.HasOne(x => x.Categoria)
               .WithMany(c => c.Productos)
               .HasForeignKey(x => x.CategoriaId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
