using FluentAssertions;
using CoreSysHM.Domain.Security;
using Xunit;

namespace CoreSysHM.UnitTests.Security;

public class RolePermissionsTests
{
    [Fact]
    public void Administrador_ForRole_IncluyeTodosLosPermisosDelCatalogo()
    {
        var permisos = RolePermissions.ForRole(RoleNames.Administrador);

        permisos.Should().BeEquivalentTo(Permissions.All());
    }

    [Fact]
    public void Administrativo_ForRole_IncluyePermisosPorDefectoDefinidos()
    {
        var permisos = RolePermissions.ForRole(RoleNames.Administrativo);

        permisos.Should().Contain(Permissions.Productos.View);
        permisos.Should().Contain(Permissions.Ventas.Create);
        permisos.Should().Contain(Permissions.Ventas.Anular);
        permisos.Should().Contain(Permissions.Reportes.Exportar);
        permisos.Should().NotContain(Permissions.Usuarios.View);
        permisos.Should().NotContain(Permissions.Roles.Edit);
        permisos.Should().NotContain(Permissions.Seguridad.Manage);
    }

    [Fact]
    public void Cliente_ForRole_IncluyeSoloPermisosRestringidos()
    {
        var permisos = RolePermissions.ForRole(RoleNames.Cliente);

        permisos.Should().NotContain(Permissions.Usuarios.View);
        permisos.Should().NotContain(Permissions.Roles.View);
        permisos.Should().NotContain(Permissions.Seguridad.Manage);
        // Cliente no debe heredar el set operativo amplio de Administrativo
        permisos.Should().NotBeEquivalentTo(RolePermissions.ForRole(RoleNames.Administrativo));
    }

    [Fact]
    public void RoleGrants_DevuelveTrue_ParaAdministrador_EnCualquierPermiso()
    {
        RolePermissions.RoleGrants(RoleNames.Administrador, "un.permiso.que.no.existe.todavia")
            .Should().BeTrue();
    }

    [Fact]
    public void RoleGrants_DevuelveFalse_ParaRolSinElPermisoConsultado()
    {
        RolePermissions.RoleGrants(RoleNames.Cliente, Permissions.Usuarios.Delete)
            .Should().BeFalse();
    }
}
