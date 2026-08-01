using Microsoft.AspNetCore.Identity;

namespace CoreSysHM.Domain.Entities.Auth;

public class ApplicationRole : IdentityRole<int>
{
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Rol de sistema (Administrador): acceso total implícito, no editable ni eliminable.
    /// Los roles protegidos sembrados de fábrica (Administrativo, Cliente) NO usan este flag
    /// -- son eliminables=false por tener IsSeeded=true, pero sí editables. Ver reglas de negocio
    /// en RoleManagementService.
    /// </summary>
    public bool IsSystem { get; set; }

    /// <summary>
    /// Roles sembrados de fábrica (Administrador, Administrativo, Cliente): no se pueden eliminar
    /// aunque no sean de sistema. Los roles creados por el Administrador sí son eliminables
    /// (si no tienen usuarios asignados).
    /// </summary>
    public bool IsSeeded { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<string> Permissions { get; set; } = new();
}
