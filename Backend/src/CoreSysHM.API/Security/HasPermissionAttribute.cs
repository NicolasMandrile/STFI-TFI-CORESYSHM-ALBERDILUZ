using Microsoft.AspNetCore.Authorization;

namespace CoreSysHM.API.Security;

/// <summary>
/// [HasPermission("usuarios.view")] -- azúcar sintáctico sobre [Authorize(Policy = "Permission:...")].
/// La política se arma dinámicamente en PermissionPolicyProvider, no hace falta registrarla a mano
/// en Program.cs por cada clave del catálogo.
/// </summary>
public class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(string permission) : base(policy: $"Permission:{permission}") { }
}
