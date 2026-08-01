using Microsoft.AspNetCore.Identity;

namespace CoreSysHM.Domain.Entities.Auth;

public class ApplicationUser : IdentityUser<int>
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime? UltimoAcceso { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
