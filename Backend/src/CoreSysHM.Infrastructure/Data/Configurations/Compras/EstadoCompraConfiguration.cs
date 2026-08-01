using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoreSysHM.Domain.Entities.Compras;

namespace CoreSysHM.Infrastructure.Data.Configurations.Compras;

public class EstadoCompraConfiguration : IEntityTypeConfiguration<EstadoCompra>
{
    private static readonly DateTime SeedDate = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public void Configure(EntityTypeBuilder<EstadoCompra> builder)
    {
        builder.ToTable("EstadosCompra");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Descripcion).IsRequired().HasMaxLength(50);

        builder.HasData(
            new EstadoCompra { Id = 1, Descripcion = "Confirmada", Activo = true, FechaCreacion = SeedDate },
            new EstadoCompra { Id = 2, Descripcion = "Anulada",    Activo = true, FechaCreacion = SeedDate },
            new EstadoCompra { Id = 3, Descripcion = "Pendiente",  Activo = true, FechaCreacion = SeedDate }
        );
    }
}
