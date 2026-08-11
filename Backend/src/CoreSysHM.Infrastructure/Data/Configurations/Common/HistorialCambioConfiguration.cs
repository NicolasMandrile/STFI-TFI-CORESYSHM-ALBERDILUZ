using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoreSysHM.Domain.Entities.Common;

namespace CoreSysHM.Infrastructure.Data.Configurations.Common;

public class HistorialCambioConfiguration : IEntityTypeConfiguration<HistorialCambio>
{
    public void Configure(EntityTypeBuilder<HistorialCambio> builder)
    {
        builder.ToTable("HistorialCambios");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Entidad).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Accion).IsRequired().HasMaxLength(30);
        builder.HasOne(x => x.Usuario)
               .WithMany()
               .HasForeignKey(x => x.UsuarioId)
               .OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(x => new { x.Entidad, x.EntidadId }).HasDatabaseName("IX_HistorialCambios_Entidad");
        builder.HasIndex(x => x.Fecha).HasDatabaseName("IX_HistorialCambios_Fecha");
    }
}
