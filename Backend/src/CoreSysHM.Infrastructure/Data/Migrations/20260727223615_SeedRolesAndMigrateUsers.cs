using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreSysHM.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedRolesAndMigrateUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Sembrar los 3 roles del sistema. Los permisos de Administrativo/Cliente acá
            // reflejan RolePermissions.ForRole(...) al momento de escribir esta migración -- son
            // el estado INICIAL, editable luego por el Administrador vía RoleManagementService.
            migrationBuilder.Sql(@"
                DECLARE @AdministrativoPermisos NVARCHAR(MAX) = N'[""productos.view"",""productos.create"",""productos.edit"",""productos.delete"",""categorias.view"",""categorias.create"",""categorias.edit"",""categorias.delete"",""proveedores.view"",""proveedores.create"",""proveedores.edit"",""proveedores.delete"",""stock.view"",""stock.registrar"",""ventas.view"",""ventas.create"",""ventas.anular"",""clientes.view"",""clientes.create"",""clientes.edit"",""compras.view"",""compras.create"",""compras.anular"",""facturas.view"",""facturas.anular"",""reportes.ver"",""reportes.exportar""]';
                DECLARE @ClientePermisos NVARCHAR(MAX) = N'[""ventas.view"",""ventas.create"",""productos.view""]';

                INSERT INTO AspNetRoles (Name, NormalizedName, ConcurrencyStamp, Description, IsActive, IsSystem, IsSeeded, CreatedAt, Permissions)
                VALUES
                (N'Administrador', N'ADMINISTRADOR', CONVERT(nvarchar(max), NEWID()), N'Acceso total al sistema. Rol de sistema, no editable ni eliminable.', 1, 1, 1, SYSUTCDATETIME(), N'[]'),
                (N'Administrativo', N'ADMINISTRATIVO', CONVERT(nvarchar(max), NEWID()), N'Acceso operativo a los módulos de negocio. Rol protegido, editable.', 1, 0, 1, SYSUTCDATETIME(), @AdministrativoPermisos),
                (N'Cliente', N'CLIENTE', CONVERT(nvarchar(max), NEWID()), N'Acceso restringido a información propia. Rol protegido, editable.', 1, 0, 1, SYSUTCDATETIME(), @ClientePermisos);
            ");

            // 2) Copiar Usuarios -> AspNetUsers preservando el Id (Compra.RegistradoPorId depende
            // de que los Id se mantengan idénticos). Si la tabla Usuarios está vacía (BD nueva sin
            // datos previos), este INSERT simplemente no copia filas.
            migrationBuilder.Sql(@"
                SET IDENTITY_INSERT dbo.AspNetUsers ON;

                INSERT INTO AspNetUsers
                    (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed,
                     PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed,
                     TwoFactorEnabled, LockoutEnabled, AccessFailedCount,
                     Nombre, Apellido, IsActive, CreatedAt)
                SELECT
                    Id, NombreUsuario, UPPER(NombreUsuario), Email, UPPER(Email), 1,
                    PasswordHash, CONVERT(nvarchar(max), NEWID()), CONVERT(nvarchar(max), NEWID()), 0,
                    0, 1, 0,
                    Nombre, Apellido, Activo, FechaCreacion
                FROM Usuarios;

                SET IDENTITY_INSERT dbo.AspNetUsers OFF;

                -- Solo reseedear si se copió al menos una fila: en una BD nueva (Usuarios vacía)
                -- AspNetUsers queda vacía acá, y reseedear una tabla vacía a 0 hace que SQL Server
                -- le asigne Id=0 a la PRIMERA fila insertada después (en vez de 1), lo que rompe
                -- IdentityUserRole<int> más adelante (Id=0 colisiona con el sentinel de EF Core
                -- para keys aún no generadas). Sin este INSERT, la IDENTITY(1,1) recién creada ya
                -- arranca en 1 por sí sola, así que no hace falta reseedear.
                IF EXISTS (SELECT 1 FROM AspNetUsers)
                BEGIN
                    DECLARE @MaxId INT = (SELECT MAX(Id) FROM AspNetUsers);
                    DECLARE @Sql NVARCHAR(200) = N'DBCC CHECKIDENT (''AspNetUsers'', RESEED, ' + CAST(@MaxId AS NVARCHAR(20)) + ')';
                    EXEC sp_executesql @Sql;
                END
            ");

            // 3) Mapear el rol viejo (enum RolUsuario: Administrador=1, Operador=2, Supervisor=3)
            // al rol nuevo correspondiente: Administrador->Administrador, Operador/Supervisor
            // (fusionados) -> Administrativo.
            migrationBuilder.Sql(@"
                DECLARE @RolAdminId INT = (SELECT Id FROM AspNetRoles WHERE NormalizedName = N'ADMINISTRADOR');
                DECLARE @RolAdministrativoId INT = (SELECT Id FROM AspNetRoles WHERE NormalizedName = N'ADMINISTRATIVO');

                INSERT INTO AspNetUserRoles (UserId, RoleId)
                SELECT u.Id, @RolAdminId FROM Usuarios u WHERE u.Rol = 1;

                INSERT INTO AspNetUserRoles (UserId, RoleId)
                SELECT u.Id, @RolAdministrativoId FROM Usuarios u WHERE u.Rol IN (2, 3);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rollback best-effort: borra lo que esta migración insertó. No intenta reconstruir
            // datos de usuarios/roles creados DESPUÉS de aplicar esta migración (fuera de alcance
            // para una migración de datos de un proyecto en desarrollo).
            migrationBuilder.Sql(@"
                DELETE FROM AspNetUserRoles WHERE UserId IN (SELECT Id FROM Usuarios);
                DELETE FROM AspNetUsers WHERE Id IN (SELECT Id FROM Usuarios);
                DELETE FROM AspNetRoles WHERE NormalizedName IN (N'ADMINISTRADOR', N'ADMINISTRATIVO', N'CLIENTE');
            ");
        }
    }
}
