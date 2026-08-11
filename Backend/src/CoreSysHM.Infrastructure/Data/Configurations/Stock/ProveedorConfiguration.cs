using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoreSysHM.Domain.Entities.Stock;

namespace CoreSysHM.Infrastructure.Data.Configurations.Stock;

public class ProveedorConfiguration : IEntityTypeConfiguration<Proveedor>
{
    public void Configure(EntityTypeBuilder<Proveedor> builder)
    {
        builder.Property(x => x.RazonSocial).IsRequired().HasMaxLength(150);
        builder.Property(x => x.Cuit).IsRequired().HasMaxLength(20);
        builder.Property(x => x.Telefono).HasMaxLength(30);
        builder.Property(x => x.Email).HasMaxLength(150);
        // .IsRowVersion() se aplica condicionalmente en ApplicationDbContext.OnModelCreating
        // (solo SQL Server la soporta de forma nativa; rompe el esquema de los tests con Sqlite).

        builder.HasOne(x => x.CondicionFiscal)
               .WithMany()
               .HasForeignKey(x => x.CondicionFiscalId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.Restrict);

        // Unicidad fiscal entre proveedores activos (mismo criterio que Cliente: la baja lógica
        // libera el Cuit para un alta nueva sin perder el historial del proveedor dado de baja).
        builder.HasIndex(x => x.Cuit)
               .IsUnique()
               .HasFilter("[Activo] = 1")
               .HasDatabaseName("IX_Proveedores_Cuit");
    }
}
