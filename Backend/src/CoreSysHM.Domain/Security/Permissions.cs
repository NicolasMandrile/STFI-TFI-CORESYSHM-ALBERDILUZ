using System.Reflection;

namespace CoreSysHM.Domain.Security;

/// <summary>
/// Catálogo único de claves de permiso del sistema, organizado por categoría/módulo.
/// Es la única fuente de verdad de "qué permisos existen" -- All() se calcula por reflexión
/// para que Administrador (RolePermissions.ForRole) nunca quede desactualizado si se agrega
/// una categoría nueva acá.
/// </summary>
public static class Permissions
{
    public static class Dashboard
    {
        public const string View = "dashboard.view";
    }

    public static class Usuarios
    {
        public const string View = "usuarios.view";
        public const string Create = "usuarios.create";
        public const string Edit = "usuarios.edit";
        public const string Delete = "usuarios.delete";
        public const string ResetPassword = "usuarios.reset_password";
    }

    public static class Roles
    {
        public const string View = "roles.view";
        public const string Create = "roles.create";
        public const string Edit = "roles.edit";
        public const string Delete = "roles.delete";
    }

    public static class Seguridad
    {
        public const string View = "security.view";
        public const string Manage = "security.manage";
    }

    public static class Productos
    {
        public const string View = "productos.view";
        public const string Create = "productos.create";
        public const string Edit = "productos.edit";
        public const string Delete = "productos.delete";
    }

    public static class Categorias
    {
        public const string View = "categorias.view";
        public const string Create = "categorias.create";
        public const string Edit = "categorias.edit";
        public const string Delete = "categorias.delete";
    }

    public static class Proveedores
    {
        public const string View = "proveedores.view";
        public const string Create = "proveedores.create";
        public const string Edit = "proveedores.edit";
        public const string Delete = "proveedores.delete";
    }

    public static class Stock
    {
        public const string View = "stock.view";
        public const string Registrar = "stock.registrar";
    }

    public static class Ventas
    {
        public const string View = "ventas.view";
        public const string Create = "ventas.create";
        public const string Anular = "ventas.anular";
    }

    public static class Clientes
    {
        public const string View = "clientes.view";
        public const string Create = "clientes.create";
        public const string Edit = "clientes.edit";
        public const string Delete = "clientes.delete";
    }

    public static class Compras
    {
        public const string View = "compras.view";
        public const string Create = "compras.create";
        public const string Anular = "compras.anular";
    }

    public static class Facturas
    {
        public const string View = "facturas.view";
        public const string Create = "facturas.create";
        public const string Anular = "facturas.anular";
    }

    public static class Reportes
    {
        public const string Ver = "reportes.ver";
        public const string Exportar = "reportes.exportar";
    }

    /// <summary>
    /// Todas las claves de permiso del catálogo, obtenidas por reflexión sobre las clases
    /// anidadas (categorías) de constantes string. Administrador siempre recibe este set completo.
    /// </summary>
    public static IReadOnlyList<string> All()
    {
        var claves = new List<string>();
        foreach (var categoria in typeof(Permissions).GetNestedTypes(BindingFlags.Public | BindingFlags.Static))
        {
            foreach (var campo in categoria.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy))
            {
                if (campo.IsLiteral && campo.FieldType == typeof(string))
                {
                    claves.Add((string)campo.GetRawConstantValue()!);
                }
            }
        }
        return claves;
    }
}
