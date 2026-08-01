using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using CoreSysHM.Application.DTOs.Auth;
using CoreSysHM.Application.DTOs.Usuarios;
using CoreSysHM.Application.Interfaces.Services;
using CoreSysHM.Domain.Security;
using CoreSysHM.UnitTests.TestInfrastructure;
using Xunit;

namespace CoreSysHM.UnitTests.Usuarios;

/// <summary>
/// xUnit crea una instancia nueva de la clase de test por cada [Fact], así que el constructor
/// (que arma una BD Sqlite in-memory nueva) aísla cada test sin necesidad de un fixture compartido.
/// </summary>
public class UsuarioManagementTests : IDisposable
{
    private readonly ServiceProvider _services;
    private readonly SqliteConnection _connection;

    public UsuarioManagementTests()
    {
        (_services, _connection) = TestServiceFactory.Create();
    }

    public void Dispose()
    {
        _services.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task CrearUsuario_ConDatosValidos_RetornaExitoso()
    {
        using var scope = _services.CreateScope();
        await TestServiceFactory.SeedRolesAsync(scope.ServiceProvider);
        var service = scope.ServiceProvider.GetRequiredService<IUserManagementService>();

        var result = await service.CreateAsync(new CreateUsuarioDto
        {
            NombreUsuario = "nuevo1",
            Email = "nuevo1@test.com",
            Password = "Password1!",
            Nombre = "Nuevo",
            Apellido = "Uno",
            Rol = RoleNames.Cliente
        });

        result.Exitoso.Should().BeTrue();
        result.Data!.Rol.Should().Be(RoleNames.Cliente);
    }

    [Fact]
    public async Task CrearUsuario_ConEmailDuplicado_RetornaError()
    {
        using var scope = _services.CreateScope();
        await TestServiceFactory.SeedRolesAsync(scope.ServiceProvider);
        var service = scope.ServiceProvider.GetRequiredService<IUserManagementService>();

        var dto = new CreateUsuarioDto
        {
            NombreUsuario = "duplicado1", Email = "duplicado@test.com", Password = "Password1!",
            Nombre = "A", Apellido = "B", Rol = RoleNames.Cliente
        };
        (await service.CreateAsync(dto)).Exitoso.Should().BeTrue();

        var segundo = new CreateUsuarioDto
        {
            NombreUsuario = "duplicado2", Email = "duplicado@test.com", Password = "Password1!",
            Nombre = "C", Apellido = "D", Rol = RoleNames.Cliente
        };
        var result = await service.CreateAsync(segundo);

        result.Exitoso.Should().BeFalse();
    }

    [Fact]
    public async Task CrearUsuario_ConPasswordSinRequisitos_RetornaError()
    {
        using var scope = _services.CreateScope();
        await TestServiceFactory.SeedRolesAsync(scope.ServiceProvider);
        var service = scope.ServiceProvider.GetRequiredService<IUserManagementService>();

        var result = await service.CreateAsync(new CreateUsuarioDto
        {
            NombreUsuario = "debil1", Email = "debil@test.com", Password = "123",
            Nombre = "Debil", Apellido = "Password", Rol = RoleNames.Cliente
        });

        result.Exitoso.Should().BeFalse();
        result.Errores.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Desactivar_UnicoAdministradorActivo_RetornaError()
    {
        using var scope = _services.CreateScope();
        await TestServiceFactory.SeedRolesAsync(scope.ServiceProvider);
        var service = scope.ServiceProvider.GetRequiredService<IUserManagementService>();

        var admin = await service.CreateAsync(new CreateUsuarioDto
        {
            NombreUsuario = "unicoadmin", Email = "unicoadmin@test.com", Password = "Password1!",
            Nombre = "Unico", Apellido = "Admin", Rol = RoleNames.Administrador
        });

        var result = await service.ToggleActivoAsync(admin.Data!.Id, activo: false);

        result.Exitoso.Should().BeFalse();
    }

    [Fact]
    public async Task Login_UsuarioInactivo_RetornaError()
    {
        using var scope = _services.CreateScope();
        await TestServiceFactory.SeedRolesAsync(scope.ServiceProvider);
        var userService = scope.ServiceProvider.GetRequiredService<IUserManagementService>();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

        var creado = await userService.CreateAsync(new CreateUsuarioDto
        {
            NombreUsuario = "inactivo1", Email = "inactivo1@test.com", Password = "Password1!",
            Nombre = "In", Apellido = "Activo", Rol = RoleNames.Cliente
        });
        await userService.ToggleActivoAsync(creado.Data!.Id, activo: false);

        var loginResult = await authService.LoginAsync(
            new LoginRequestDto { Email = "inactivo1@test.com", Password = "Password1!" }, null, null);

        loginResult.Exitoso.Should().BeFalse();
    }
}
