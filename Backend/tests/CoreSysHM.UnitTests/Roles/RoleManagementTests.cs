using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using CoreSysHM.Application.DTOs.Roles;
using CoreSysHM.Application.DTOs.Usuarios;
using CoreSysHM.Application.Interfaces.Services;
using CoreSysHM.Domain.Security;
using CoreSysHM.UnitTests.TestInfrastructure;
using Xunit;

namespace CoreSysHM.UnitTests.Roles;

public class RoleManagementTests : IDisposable
{
    private readonly ServiceProvider _services;
    private readonly SqliteConnection _connection;

    public RoleManagementTests()
    {
        (_services, _connection) = TestServiceFactory.Create();
    }

    public void Dispose()
    {
        _services.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task CrearRolPersonalizado_RetornaExitoso()
    {
        using var scope = _services.CreateScope();
        await TestServiceFactory.SeedRolesAsync(scope.ServiceProvider);
        var service = scope.ServiceProvider.GetRequiredService<IRoleManagementService>();

        var result = await service.CreateAsync(new CreateRoleDto
        {
            Name = "Vendedor",
            Description = "Rol personalizado de prueba",
            Permissions = new List<string> { Permissions.Ventas.View, Permissions.Ventas.Create }
        });

        result.Exitoso.Should().BeTrue();
        result.Data!.IsSystem.Should().BeFalse();
        result.Data!.IsSeeded.Should().BeFalse();
    }

    [Fact]
    public async Task CrearRol_ConNombreDuplicado_RetornaError()
    {
        using var scope = _services.CreateScope();
        await TestServiceFactory.SeedRolesAsync(scope.ServiceProvider);
        var service = scope.ServiceProvider.GetRequiredService<IRoleManagementService>();

        var result = await service.CreateAsync(new CreateRoleDto { Name = RoleNames.Administrativo });

        result.Exitoso.Should().BeFalse();
    }

    [Fact]
    public async Task EliminarRolDeSistema_RetornaError()
    {
        using var scope = _services.CreateScope();
        await TestServiceFactory.SeedRolesAsync(scope.ServiceProvider);
        var service = scope.ServiceProvider.GetRequiredService<IRoleManagementService>();

        var administrador = (await service.GetAllAsync()).Data!.Single(r => r.Name == RoleNames.Administrador);
        var result = await service.DeleteAsync(administrador.Id);

        result.Exitoso.Should().BeFalse();
    }

    [Fact]
    public async Task EliminarRolProtegido_RetornaError()
    {
        using var scope = _services.CreateScope();
        await TestServiceFactory.SeedRolesAsync(scope.ServiceProvider);
        var service = scope.ServiceProvider.GetRequiredService<IRoleManagementService>();

        var administrativo = (await service.GetAllAsync()).Data!.Single(r => r.Name == RoleNames.Administrativo);
        var result = await service.DeleteAsync(administrativo.Id);

        result.Exitoso.Should().BeFalse();
    }

    [Fact]
    public async Task EliminarRolConUsuariosAsignados_RetornaError()
    {
        using var scope = _services.CreateScope();
        await TestServiceFactory.SeedRolesAsync(scope.ServiceProvider);
        var roleService = scope.ServiceProvider.GetRequiredService<IRoleManagementService>();
        var userService = scope.ServiceProvider.GetRequiredService<IUserManagementService>();

        var rol = await roleService.CreateAsync(new CreateRoleDto { Name = "ConUsuarios" });
        await userService.CreateAsync(new CreateUsuarioDto
        {
            NombreUsuario = "asignado1", Email = "asignado1@test.com", Password = "Password1!",
            Nombre = "A", Apellido = "B", Rol = "ConUsuarios"
        });

        var result = await roleService.DeleteAsync(rol.Data!.Id);

        result.Exitoso.Should().BeFalse();
    }

    [Fact]
    public async Task EditarGrupoDeAcciones_DeRolNoSistema_RetornaExitoso()
    {
        using var scope = _services.CreateScope();
        await TestServiceFactory.SeedRolesAsync(scope.ServiceProvider);
        var service = scope.ServiceProvider.GetRequiredService<IRoleManagementService>();

        var administrativo = (await service.GetAllAsync()).Data!.Single(r => r.Name == RoleNames.Administrativo);

        var result = await service.UpdateAsync(administrativo.Id, new UpdateRoleDto
        {
            Name = administrativo.Name,
            Description = administrativo.Description,
            IsActive = true,
            Permissions = new List<string> { Permissions.Reportes.Ver }
        });

        result.Exitoso.Should().BeTrue();
        result.Data!.Permissions.Should().BeEquivalentTo(new[] { Permissions.Reportes.Ver });
    }
}
