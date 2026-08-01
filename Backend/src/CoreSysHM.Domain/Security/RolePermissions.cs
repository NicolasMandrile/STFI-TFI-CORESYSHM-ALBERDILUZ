namespace CoreSysHM.Domain.Security;

/// <summary>
/// Fuente de verdad de los permisos DEFAULT por rol -- se usa para el seed inicial (migración de
/// datos / DbInitializer) y para los tests unitarios de "cuáles son los permisos por defecto
/// esperados de cada rol".
///
/// IMPORTANTE: en runtime (login, autorización), los permisos EFECTIVOS de un rol se leen de
/// ApplicationRole.Permissions (columna JSON editable por el Administrador vía RoleManagementService),
/// nunca de esta clase. Esta clase solo importa en el momento de creación/seed del rol y en tests.
/// </summary>
public static class RolePermissions
{
    private static readonly IReadOnlyList<string> AdministrativoDefaults = new[]
    {
        Permissions.Dashboard.View,
        Permissions.Productos.View, Permissions.Productos.Create, Permissions.Productos.Edit, Permissions.Productos.Delete,
        Permissions.Categorias.View, Permissions.Categorias.Create, Permissions.Categorias.Edit, Permissions.Categorias.Delete,
        Permissions.Proveedores.View, Permissions.Proveedores.Create, Permissions.Proveedores.Edit, Permissions.Proveedores.Delete,
        Permissions.Stock.View, Permissions.Stock.Registrar,
        Permissions.Ventas.View, Permissions.Ventas.Create, Permissions.Ventas.Anular,
        Permissions.Clientes.View, Permissions.Clientes.Create, Permissions.Clientes.Edit,
        Permissions.Compras.View, Permissions.Compras.Create, Permissions.Compras.Anular,
        Permissions.Facturas.View, Permissions.Facturas.Anular,
        Permissions.Reportes.Ver, Permissions.Reportes.Exportar,
    };

    // Cliente es un portal restringido: ve el historial de SUS PROPIAS ventas y puede
    // autogestionar la carga de una venta propia (Cliente.UserId vinculado -- ver
    // VentaService.GetClienteIdByUserIdAsync). VentasController.Create ignora cualquier
    // ClienteId recibido en el body para este rol y fuerza el del cliente de negocio vinculado
    // al login -- nunca puede registrar una venta a nombre de otro cliente. Productos.View le
    // permite ver el catálogo para armar el carrito; ProductosController oculta PrecioCompra/
    // Proveedor* en la respuesta para este rol. Sigue sin incluir usuarios.*/roles.*/security.*
    // ni ningún otro permiso de escritura bajo ninguna circunstancia.
    private static readonly IReadOnlyList<string> ClienteDefaults = new[]
    {
        Permissions.Ventas.View,
        Permissions.Ventas.Create,
        Permissions.Productos.View,
    };

    /// <summary>Permisos default del rol al momento de sembrarlo. Administrador = catálogo completo.</summary>
    public static IReadOnlyList<string> ForRole(string role) => role switch
    {
        RoleNames.Administrador => Permissions.All(),
        RoleNames.Administrativo => AdministrativoDefaults,
        RoleNames.Cliente => ClienteDefaults,
        _ => Array.Empty<string>()
    };

    /// <summary>¿El rol (por sus permisos default) otorga este permiso puntual?</summary>
    public static bool RoleGrants(string role, string permission) =>
        role == RoleNames.Administrador || ForRole(role).Contains(permission);
}
