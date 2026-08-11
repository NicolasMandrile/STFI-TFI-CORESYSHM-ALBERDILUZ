using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoreSysHM.Domain.Entities.Ventas;

namespace CoreSysHM.Infrastructure.Data.Configurations.Ventas;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.Property(x => x.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Apellido).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Dni).HasMaxLength(20);
        builder.Property(x => x.Cuit).HasMaxLength(20);
        builder.Property(x => x.Email).HasMaxLength(150);
        builder.Property(x => x.Telefono).HasMaxLength(30);
        // .IsRowVersion() se aplica condicionalmente en ApplicationDbContext.OnModelCreating
        // (solo SQL Server la soporta de forma nativa; rompe el esquema de los tests con Sqlite).

        builder.HasOne(x => x.CondicionFiscal)
               .WithMany()
               .HasForeignKey(x => x.CondicionFiscalId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.Restrict);

        // Unicidad fiscal: Dni y Cuit únicos entre los clientes activos que los tienen cargados
        // (ambos son opcionales -- un cliente puede tener uno, el otro, o ninguno todavía).
        builder.HasIndex(x => x.Dni)
               .IsUnique()
               .HasFilter("[Dni] IS NOT NULL AND [Activo] = 1")
               .HasDatabaseName("IX_Clientes_Dni");
        builder.HasIndex(x => x.Cuit)
               .IsUnique()
               .HasFilter("[Cuit] IS NOT NULL AND [Activo] = 1")
               .HasDatabaseName("IX_Clientes_Cuit");
    }
}
