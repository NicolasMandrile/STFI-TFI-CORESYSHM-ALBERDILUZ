using Microsoft.AspNetCore.Authorization;
using CoreSysHM.Domain.Security;

namespace CoreSysHM.API.Security;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        // Administrador pasa siempre por ROL, no por la presencia individual del permiso en el
        // token: así nunca queda afuera aunque el catálogo de permisos cambie a futuro.
        if (context.User.IsInRole(RoleNames.Administrador) ||
            context.User.HasClaim(c => c.Type == "permission" && c.Value == requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
