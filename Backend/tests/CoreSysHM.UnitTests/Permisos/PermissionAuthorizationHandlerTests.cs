using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using CoreSysHM.API.Security;
using CoreSysHM.Domain.Security;
using Xunit;

namespace CoreSysHM.UnitTests.Permisos;

/// <summary>
/// Cubre a nivel backend el equivalente de "guards y permisos": el handler de autorización es la
/// pieza que efectivamente devuelve 401/403. Los casos de guards de Angular (redirección a
/// login/acceso-denegado) se cubren en los specs del frontend (Fase 8).
/// </summary>
public class PermissionAuthorizationHandlerTests
{
    private static ClaimsPrincipal BuildUser(string? rol, params string[] permisos)
    {
        var claims = new List<Claim>();
        if (rol != null) claims.Add(new Claim(ClaimTypes.Role, rol));
        claims.AddRange(permisos.Select(p => new Claim("permission", p)));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    }

    private static async Task<AuthorizationHandlerContext> RunAsync(ClaimsPrincipal user, string permisoRequerido)
    {
        var handler = new PermissionAuthorizationHandler();
        var requirement = new PermissionRequirement(permisoRequerido);
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);
        await handler.HandleAsync(context);
        return context;
    }

    [Fact]
    public async Task Administrador_AccedeSiempre_AunSinElPermisoEnElToken()
    {
        var user = BuildUser(RoleNames.Administrador); // sin claims de permission

        var context = await RunAsync(user, Permissions.Usuarios.Delete);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task UsuarioConElPermisoJusto_Accede()
    {
        var user = BuildUser(RoleNames.Administrativo, Permissions.Ventas.View);

        var context = await RunAsync(user, Permissions.Ventas.View);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task PermisoNoAsignadoAlRol_NoAutoriza()
    {
        var user = BuildUser(RoleNames.Cliente); // Cliente sin permisos

        var context = await RunAsync(user, Permissions.Usuarios.View);

        context.HasSucceeded.Should().BeFalse();
    }
}
