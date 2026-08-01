using System.Text.Json;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using CoreSysHM.Domain.Entities.Auth;
using CoreSysHM.Domain.Entities.Compras;
using CoreSysHM.Domain.Entities.Facturacion;
using CoreSysHM.Domain.Entities.Stock;
using CoreSysHM.Domain.Entities.Ventas;

namespace CoreSysHM.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<AuditoriaAcceso> AuditoriasAcceso => Set<AuditoriaAcceso>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<MovimientoStock> MovimientosStock => Set<MovimientoStock>();
    public DbSet<EstadoCompra> EstadosCompra => Set<EstadoCompra>();
    public DbSet<Compra> Compras => Set<Compra>();
    public DbSet<DetalleCompra> DetallesCompra => Set<DetalleCompra>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<DetalleVenta> DetallesVenta => Set<DetalleVenta>();
    public DbSet<Factura> Facturas => Set<Factura>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // Configura las tablas AspNet* -- debe ir primero
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // ApplicationRole.Permissions: List<string> -> nvarchar(max) JSON. Requiere ValueComparer
        // explícito para que el ChangeTracker detecte mutaciones dentro de la lista (Add/Remove).
        var permissionsComparer = new ValueComparer<List<string>>(
            (a, b) => (a ?? new()).SequenceEqual(b ?? new()),
            p => p.Aggregate(0, (hash, s) => HashCode.Combine(hash, s.GetHashCode())),
            p => p.ToList());

        modelBuilder.Entity<ApplicationRole>(b =>
        {
            b.Property(r => r.Description).HasMaxLength(200);
            // Sin HasColumnType explícito a propósito: "nvarchar(max)" es específico de SQL Server
            // y rompe el esquema cuando los tests corren contra Sqlite in-memory. EF ya mapea un
            // string sin longitud máxima a nvarchar(max) por default en el provider de SqlServer.
            b.Property(r => r.Permissions)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>())
                .Metadata.SetValueComparer(permissionsComparer);
        });

        modelBuilder.Entity<ApplicationUser>(b =>
        {
            b.Property(u => u.Nombre).HasMaxLength(100).IsRequired();
            b.Property(u => u.Apellido).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<AuditoriaAcceso>(b =>
        {
            b.HasKey(a => a.Id);
            b.Property(a => a.RolSnapshot).HasMaxLength(50);
            b.Property(a => a.Ip).HasMaxLength(64);
            b.Property(a => a.UserAgent).HasMaxLength(500);
            b.Property(a => a.Detalle).HasMaxLength(500);
            b.HasOne(a => a.Usuario)
                .WithMany()
                .HasForeignKey(a => a.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull);
            b.HasIndex(a => a.Timestamp).HasDatabaseName("IX_AuditoriaAcceso_Timestamp");
            b.HasIndex(a => a.UsuarioId).HasDatabaseName("IX_AuditoriaAcceso_UsuarioId");
        });

        // Proveedor → Producto (optional FK)
        modelBuilder.Entity<Producto>()
            .HasOne(p => p.Proveedor)
            .WithMany(pv => pv.Productos)
            .HasForeignKey(p => p.ProveedorId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // EstadoCompra → Compra
        modelBuilder.Entity<Compra>()
            .HasOne(c => c.EstadoCompra)
            .WithMany(e => e.Compras)
            .HasForeignKey(c => c.EstadoCompraId)
            .OnDelete(DeleteBehavior.Restrict);

        // ApplicationUser → Compra (administrativo que registra, opcional)
        modelBuilder.Entity<Compra>()
            .HasOne(c => c.RegistradoPor)
            .WithMany()
            .HasForeignKey(c => c.RegistradoPorId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // Compra → DetalleCompra
        modelBuilder.Entity<DetalleCompra>()
            .HasOne(d => d.Compra)
            .WithMany(c => c.Detalles)
            .HasForeignKey(d => d.CompraId)
            .OnDelete(DeleteBehavior.Cascade);

        // ApplicationUser → Cliente (portal de cliente, opcional, 1 login <-> 1 cliente como máximo)
        modelBuilder.Entity<Cliente>()
            .HasOne(c => c.Usuario)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Cliente>()
            .HasIndex(c => c.UserId)
            .IsUnique()
            .HasFilter("[UserId] IS NOT NULL")
            .HasDatabaseName("IX_Clientes_UserId");

        // Decimal precision
        modelBuilder.Entity<Compra>().Property(c => c.Total).HasPrecision(18, 2);
        modelBuilder.Entity<DetalleCompra>().Property(d => d.PrecioUnitario).HasPrecision(18, 2);
        modelBuilder.Entity<DetalleCompra>().Property(d => d.Subtotal).HasPrecision(18, 2);

        // Índices para filtros de reportes de compras
        modelBuilder.Entity<Compra>()
            .HasIndex(c => c.Fecha).HasDatabaseName("IX_Compras_Fecha");
        modelBuilder.Entity<Compra>()
            .HasIndex(c => c.ProveedorId).HasDatabaseName("IX_Compras_ProveedorId");
        modelBuilder.Entity<Compra>()
            .HasIndex(c => c.EstadoCompraId).HasDatabaseName("IX_Compras_EstadoCompraId");
        modelBuilder.Entity<DetalleCompra>()
            .HasIndex(d => d.ProductoId).HasDatabaseName("IX_DetallesCompra_ProductoId");
    }
}
