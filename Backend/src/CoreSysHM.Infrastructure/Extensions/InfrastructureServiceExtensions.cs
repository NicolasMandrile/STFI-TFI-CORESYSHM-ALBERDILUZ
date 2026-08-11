using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CoreSysHM.Application.Interfaces.Services;
using CoreSysHM.Domain.Entities.Auth;
using CoreSysHM.Domain.Interfaces;
using CoreSysHM.Domain.Interfaces.Repositories;
using CoreSysHM.Infrastructure.Data;
using CoreSysHM.Infrastructure.Repositories;
using CoreSysHM.Infrastructure.Security;
using CoreSysHM.Infrastructure.Services;

namespace CoreSysHM.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
        });

        // Identity: AddIdentityCore (no AddIdentity) para NO registrar el esquema de cookies de
        // Identity, que pisaría el AddAuthentication(JwtBearerDefaults...) ya configurado en
        // ApplicationServiceExtensions. Este proyecto es un API JWT puro.
        services.AddHttpContextAccessor();
        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.User.RequireUniqueEmail = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        // Reemplaza el PasswordHasher<ApplicationUser> default registrado por AddIdentityCore
        // (el contenedor resuelve el último registro para un tipo dado).
        services.AddScoped<IPasswordHasher<ApplicationUser>, LegacyBCryptPasswordHasher>();

        // Repositories
        services.AddScoped<IProductoRepository, ProductoRepository>();
        services.AddScoped<IVentaRepository, VentaRepository>();
        services.AddScoped<IFacturaRepository, FacturaRepository>();
        services.AddScoped<IProveedorRepository, ProveedorRepository>();
        services.AddScoped<ICompraRepository, CompraRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAuditoriaService, AuditoriaService>();
        services.AddScoped<IUserManagementService, UserManagementService>();
        services.AddScoped<IRoleManagementService, RoleManagementService>();
        services.AddScoped<IProductoService, ProductoService>();
        services.AddScoped<ICategoriaService, CategoriaService>();
        services.AddScoped<IVentaService, VentaService>();
        services.AddScoped<IMovimientoStockService, MovimientoStockService>();
        services.AddScoped<IProveedorService, ProveedorService>();
        services.AddScoped<ICompraService, CompraService>();
        services.AddScoped<IReporteService, ReporteService>();
        services.AddScoped<IReporteComprasService, ReporteComprasService>();
        services.AddScoped<IHistorialCambioService, HistorialCambioService>();
        services.AddScoped<IFacturaService, FacturaService>();
        services.AddScoped<IClienteService, ClienteService>();
        services.AddScoped<IReporteFacturacionService, ReporteFacturacionService>();

        return services;
    }
}
