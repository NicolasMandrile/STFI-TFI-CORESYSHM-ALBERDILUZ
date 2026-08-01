using Microsoft.AspNetCore.Authorization;

namespace CoreSysHM.API.Security;

public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }
    public PermissionRequirement(string permission) => Permission = permission;
}
