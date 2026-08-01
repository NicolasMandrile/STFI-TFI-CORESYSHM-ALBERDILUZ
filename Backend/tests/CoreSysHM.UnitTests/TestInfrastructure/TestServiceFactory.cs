using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CoreSysHM.Application.Interfaces.Services;
using CoreSysHM.Domain.Entities.Auth;
using CoreSysHM.Domain.Security;
using CoreSysHM.Infrastructure.Data;
using CoreSysHM.Infrastructure.Security;
using CoreSysHM.Infrastructure.Services;

namespace CoreSysHM.UnitTests.TestInfrastructure;

/// <summary>
/// Arma un contenedor de DI equivalente al de producción (mismo AddIdentityCore + hasher custom)
/// pero contra Sqlite in-memory en vez de SQL Server -- se usa EnsureCreated (no las migraciones
/// reales, que tienen SQL específico de SQL Server) para levantar el esquema desde el modelo actual.
/// Cada test crea su propia instancia via Create() para no compartir estado entre tests.
/// </summary>
internal static class TestServiceFactory
{
    public static (ServiceProvider Services, SqliteConnection Connection) Create()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(BuildConfiguration());
        services.AddDbContext<ApplicationDbContext>(o => o.UseSqlite(connection));
        services.AddHttpContextAccessor();
        services.AddDataProtection();
        services.AddAuthentication();

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

        services.AddScoped<IPasswordHasher<ApplicationUser>, LegacyBCryptPasswordHasher>();
        services.AddScoped<IAuditoriaService, AuditoriaService>();
        services.AddScoped<IUserManagementService, UserManagementService>();
        services.AddScoped<IRoleManagementService, RoleManagementService>();
        services.AddScoped<IAuthService, AuthService>();

        var provider = services.BuildServiceProvider();

        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureCreated();
        }

        return (provider, connection);
    }

    /// <summary>Replica el seed de roles de DbInitializer para los tests.</summary>
    public static async Task SeedRolesAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
        foreach (var nombreRol in RoleNames.All)
        {
            if (await roleManager.FindByNameAsync(nombreRol) is not null)
                continue;

            await roleManager.CreateAsync(new ApplicationRole
            {
                Name = nombreRol,
                IsActive = true,
                IsSystem = nombreRol == RoleNames.Administrador,
                IsSeeded = true,
                CreatedAt = DateTime.UtcNow,
                Permissions = RolePermissions.ForRole(nombreRol).ToList()
            });
        }
    }

    private static IConfiguration BuildConfiguration() =>
        new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "Test_Super_Secret_Key_For_Unit_Tests_Only_1234567890",
            ["Jwt:Issuer"] = "TestIssuer",
            ["Jwt:Audience"] = "TestAudience",
            ["Jwt:ExpirationHours"] = "1"
        }).Build();
}
