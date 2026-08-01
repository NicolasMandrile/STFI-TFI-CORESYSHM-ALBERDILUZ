using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoreSysHM.Domain.Entities.Stock;

namespace CoreSysHM.Infrastructure.Data.Configurations.Stock;

public class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        builder.ToTable("Categorias");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Descripcion).HasMaxLength(300);
        builder.HasIndex(x => x.Nombre).IsUnique();
    }
}
