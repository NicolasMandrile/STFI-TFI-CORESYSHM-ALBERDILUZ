using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CoreSysHM.Application.DTOs.Auth;
using CoreSysHM.Application.DTOs.Usuarios;
using CoreSysHM.Application.Interfaces.Services;
using CoreSysHM.Domain.Security;
using CoreSysHM.Infrastructure.Data;
using CoreSysHM.UnitTests.TestInfrastructure;
using Xunit;

namespace CoreSysHM.UnitTests.Auditoria;

public class AuditoriaTests : IDisposable
{
    private readonly ServiceProvider _services;
    private readonly SqliteConnection _connection;

    public AuditoriaTests()
    {
        (_services, _connection) = TestServiceFactory.Create();
    }

    public void Dispose()
    {
        _services.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task LoginExitoso_QuedaRegistradoEnAuditoria()
    {
        using var scope = _services.CreateScope();
        await TestServiceFactory.SeedRolesAsync(scope.ServiceProvider);
        var userService = scope.ServiceProvider.GetRequiredService<IUserManagementService>();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

        await userService.CreateAsync(new CreateUsuarioDto
        {
            NombreUsuario = "auditlogin", Email = "auditlogin@test.com", Password = "Password1!",
            Nombre = "A", Apellido = "B", Rol = RoleNames.Cliente
        });

        var result = await authService.LoginAsync(
            new LoginRequestDto { Email = "auditlogin@test.com", Password = "Password1!" }, "127.0.0.1", "xUnit");

        result.Exitoso.Should().BeTrue();

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var registrado = await db.AuditoriasAcceso.AnyAsync(a =>
            a.Accion == Domain.Enums.TipoAccionAuditoria.Login && a.Ip == "127.0.0.1");
        registrado.Should().BeTrue();
    }

    [Fact]
    public async Task LoginFallido_QuedaRegistradoEnAuditoria()
    {
        using var scope = _services.CreateScope();
        await TestServiceFactory.SeedRolesAsync(scope.ServiceProvider);
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

        var result = await authService.LoginAsync(
            new LoginRequestDto { Email = "noexiste@test.com", Password = "Password1!" }, "10.0.0.1", "xUnit");

        result.Exitoso.Should().BeFalse();

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var registrado = await db.AuditoriasAcceso.AnyAsync(a =>
            a.Accion == Domain.Enums.TipoAccionAuditoria.LoginFallido && a.Ip == "10.0.0.1");
        registrado.Should().BeTrue();
    }

    [Fact]
    public async Task ResetDePassword_QuedaRegistradoEnAuditoria()
    {
        using var scope = _services.CreateScope();
        await TestServiceFactory.SeedRolesAsync(scope.ServiceProvider);
        var userService = scope.ServiceProvider.GetRequiredService<IUserManagementService>();

        var creado = await userService.CreateAsync(new CreateUsuarioDto
        {
            NombreUsuario = "auditreset", Email = "auditreset@test.com", Password = "Password1!",
            Nombre = "A", Apellido = "B", Rol = RoleNames.Cliente
        });

        var reset = await userService.ResetPasswordAsync(creado.Data!.Id, "NuevaPassword1!");
        reset.Exitoso.Should().BeTrue();

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var registrado = await db.AuditoriasAcceso.AnyAsync(a =>
            a.Accion == Domain.Enums.TipoAccionAuditoria.ResetPassword && a.UsuarioId == creado.Data!.Id);
        registrado.Should().BeTrue();
    }
}
