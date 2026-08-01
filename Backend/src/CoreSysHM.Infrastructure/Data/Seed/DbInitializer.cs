using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CoreSysHM.Domain.Entities.Auth;
using CoreSysHM.Domain.Entities.Stock;
using CoreSysHM.Domain.Entities.Ventas;
using CoreSysHM.Domain.Security;

namespace CoreSysHM.Infrastructure.Data.Seed;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var configuration = services.GetRequiredService<IConfiguration>();

        await SeedRolesAsync(roleManager);
        await SeedAdminAsync(userManager, configuration);

        var categoriasRequeridas = new[]
        {
            ("Electrónica",  "Dispositivos y componentes electrónicos"),
            ("Herramientas", "Herramientas de trabajo y mantenimiento"),
            ("Insumos",      "Materiales e insumos de oficina"),
            ("Repuestos",    "Repuestos y autopartes"),
            ("Lubricantes",  "Aceites y lubricantes"),
        };
        foreach (var (nombre, desc) in categoriasRequeridas)
        {
            if (!await context.Categorias.AnyAsync(c => c.Nombre == nombre))
                context.Categorias.Add(new Categoria { Nombre = nombre, Descripcion = desc });
        }
        await context.SaveChangesAsync();

        if (!await context.Proveedores.AnyAsync())
        {
            context.Proveedores.AddRange(
                new Proveedor
                {
                    RazonSocial = "TechParts S.A.",
                    Cuit        = "30-71234567-8",
                    Telefono    = "011-4567-8901",
                    Email       = "ventas@techparts.com.ar",
                    Direccion   = "Av. Corrientes 1234, CABA",
                    Contacto    = "Carlos Ruiz"
                },
                new Proveedor
                {
                    RazonSocial = "Insumos del Sur S.R.L.",
                    Cuit        = "30-65432198-7",
                    Telefono    = "0341-456-7890",
                    Email       = "compras@insumossur.com.ar",
                    Direccion   = "Bv. Oroño 567, Rosario",
                    Contacto    = "Ana Martínez"
                },
                new Proveedor
                {
                    RazonSocial = "Repuestos Automotores Norte",
                    Cuit        = "20-28765432-1",
                    Telefono    = "011-3456-7890",
                    Email       = "info@repuestesnorte.com.ar",
                    Contacto    = "Miguel Torres"
                }
            );
            await context.SaveChangesAsync();
        }

        if (!await context.Productos.AnyAsync())
        {
            var catElec   = await context.Categorias.FirstAsync(c => c.Nombre == "Electrónica");
            var catHerr   = await context.Categorias.FirstAsync(c => c.Nombre == "Herramientas");
            var catInsu   = await context.Categorias.FirstAsync(c => c.Nombre == "Insumos");
            var catRep    = await context.Categorias.FirstAsync(c => c.Nombre == "Repuestos");
            var catLub    = await context.Categorias.FirstAsync(c => c.Nombre == "Lubricantes");
            var prov1     = await context.Proveedores.FirstAsync(p => p.RazonSocial == "TechParts S.A.");
            var prov2     = await context.Proveedores.FirstAsync(p => p.RazonSocial == "Insumos del Sur S.R.L.");
            var prov3     = await context.Proveedores.FirstAsync(p => p.RazonSocial == "Repuestos Automotores Norte");

            context.Productos.AddRange(
                new Producto { Codigo = "ELEC-001", Nombre = "Cámara de seguridad HD",      Descripcion = "Cámara 1080p con visión nocturna",  PrecioCompra = 8500,  PrecioVenta = 12000, StockActual = 15, StockMinimo = 3,  CategoriaId = catElec.Id, ProveedorId = prov1.Id },
                new Producto { Codigo = "ELEC-002", Nombre = "Disco duro externo 1TB",      Descripcion = "HDD USB 3.0 portable",               PrecioCompra = 6200,  PrecioVenta = 9500,  StockActual = 8,  StockMinimo = 2,  CategoriaId = catElec.Id, ProveedorId = prov1.Id },
                new Producto { Codigo = "ELEC-003", Nombre = "Teclado inalámbrico",         Descripcion = "Teclado bluetooth compacto",          PrecioCompra = 3100,  PrecioVenta = 5200,  StockActual = 2,  StockMinimo = 5,  CategoriaId = catElec.Id, ProveedorId = prov1.Id },
                new Producto { Codigo = "HERR-001", Nombre = "Taladro percutor 750W",       Descripcion = "Taladro de impacto 13mm",             PrecioCompra = 14000, PrecioVenta = 22000, StockActual = 6,  StockMinimo = 2,  CategoriaId = catHerr.Id, ProveedorId = prov2.Id },
                new Producto { Codigo = "HERR-002", Nombre = "Juego de llaves combinadas",  Descripcion = "Set 12 piezas 8-19mm",                PrecioCompra = 2800,  PrecioVenta = 4500,  StockActual = 12, StockMinimo = 4,  CategoriaId = catHerr.Id, ProveedorId = prov2.Id },
                new Producto { Codigo = "INSU-001", Nombre = "Resma papel A4",              Descripcion = "500 hojas 75g/m²",                    PrecioCompra = 1200,  PrecioVenta = 1800,  StockActual = 30, StockMinimo = 10, CategoriaId = catInsu.Id, ProveedorId = prov2.Id },
                new Producto { Codigo = "INSU-002", Nombre = "Tóner HP LaserJet",           Descripcion = "Compatible HP 85A",                   PrecioCompra = 4500,  PrecioVenta = 7200,  StockActual = 1,  StockMinimo = 3,  CategoriaId = catInsu.Id, ProveedorId = prov2.Id },
                new Producto { Codigo = "REP-001",  Nombre = "Filtro de aceite universal",  Descripcion = "Apto múltiples marcas",               PrecioCompra = 950,   PrecioVenta = 1600,  StockActual = 20, StockMinimo = 8,  CategoriaId = catRep.Id,  ProveedorId = prov3.Id },
                new Producto { Codigo = "REP-002",  Nombre = "Pastillas de freno delant.",  Descripcion = "Kit 4 pastillas cerámicas",           PrecioCompra = 3200,  PrecioVenta = 5500,  StockActual = 10, StockMinimo = 4,  CategoriaId = catRep.Id,  ProveedorId = prov3.Id },
                new Producto { Codigo = "LUB-001",  Nombre = "Aceite motor 10W40 4L",       Descripcion = "Aceite multigrado mineral",           PrecioCompra = 2100,  PrecioVenta = 3400,  StockActual = 3,  StockMinimo = 6,  CategoriaId = catLub.Id,  ProveedorId = prov3.Id }
            );
            await context.SaveChangesAsync();
        }

        if (!await context.Clientes.AnyAsync())
        {
            context.Clientes.AddRange(
                new Cliente { Nombre = "Roberto",   Apellido = "Sánchez",    Dni = "28456789", Email = "roberto.sanchez@gmail.com",  Telefono = "011-5678-9012", Direccion = "Tucumán 456, CABA" },
                new Cliente { Nombre = "Laura",     Apellido = "Fernández",  Dni = "31234567", Email = "laura.fernandez@outlook.com", Telefono = "0341-234-5678", Direccion = "Mendoza 789, Rosario" },
                new Cliente { Nombre = "Diego",     Apellido = "Ramírez",    Dni = "25678901", Email = "diego.ramirez@yahoo.com",    Telefono = "011-4567-0123" },
                new Cliente { Nombre = "Valentina", Apellido = "López",      Dni = "33456789", Email = "vlopez@gmail.com",           Telefono = "0351-345-6789", Direccion = "Colón 234, Córdoba" },
                new Cliente { Nombre = "Martín",    Apellido = "García",     Dni = "29876543", Email = "mgarcia@empresa.com.ar",     Telefono = "011-6789-0123", Direccion = "Av. Santa Fe 999, CABA" }
            );
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager)
    {
        // Idempotente: la migración "SeedRolesAndMigrateUsers" ya inserta estos 3 roles la primera
        // vez que se aplica. Este método solo cubre el caso de una BD creada por fuera del flujo
        // de migraciones (ej. tests de integración con Sqlite in-memory).
        foreach (var nombreRol in RoleNames.All)
        {
            if (await roleManager.FindByNameAsync(nombreRol) is not null)
                continue;

            var role = new ApplicationRole
            {
                Name = nombreRol,
                Description = nombreRol switch
                {
                    RoleNames.Administrador => "Acceso total al sistema. Rol de sistema, no editable ni eliminable.",
                    RoleNames.Administrativo => "Acceso operativo a los módulos de negocio. Rol protegido, editable.",
                    RoleNames.Cliente => "Acceso restringido a información propia. Rol protegido, editable.",
                    _ => null
                },
                IsActive = true,
                IsSystem = nombreRol == RoleNames.Administrador,
                IsSeeded = true,
                CreatedAt = DateTime.UtcNow,
                Permissions = RolePermissions.ForRole(nombreRol).ToList()
            };
            await roleManager.CreateAsync(role);
        }
    }

    private static async Task SeedAdminAsync(UserManager<ApplicationUser> userManager, IConfiguration configuration)
    {
        var admins = await userManager.GetUsersInRoleAsync(RoleNames.Administrador);
        if (admins.Any())
            return;

        var email = configuration["InitialAdmin:Email"] ?? "admin@coresyshm.com";
        var password = configuration["InitialAdmin:Password"] ?? "Admin123!";
        var nombreUsuario = configuration["InitialAdmin:UserName"] ?? "admin";

        var existente = await userManager.FindByEmailAsync(email);
        if (existente is not null)
        {
            if (!await userManager.IsInRoleAsync(existente, RoleNames.Administrador))
                await userManager.AddToRoleAsync(existente, RoleNames.Administrador);
            return;
        }

        var admin = new ApplicationUser
        {
            UserName = nombreUsuario,
            Email = email,
            EmailConfirmed = true,
            Nombre = "Administrador",
            Apellido = "Sistema",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var resultado = await userManager.CreateAsync(admin, password);
        if (resultado.Succeeded)
            await userManager.AddToRoleAsync(admin, RoleNames.Administrador);
    }
}
