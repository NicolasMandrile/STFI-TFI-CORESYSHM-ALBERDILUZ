using CoreSysHM.Domain.Enums;

namespace CoreSysHM.Domain.Entities.Auth;

/// <summary>
/// Log de accesos y acciones de seguridad. No hereda BaseEntity a propósito: BaseEntity trae
/// "Activo" (soft-delete) y este log no debe poder marcarse como borrado ni exponer un método
/// de borrado en ningún nivel (servicio/repositorio/controller).
/// </summary>
public class AuditoriaAcceso
{
    public long Id { get; set; }
    public int? UsuarioId { get; set; }
    public ApplicationUser? Usuario { get; set; }
    public string? RolSnapshot { get; set; }
    public TipoAccionAuditoria Accion { get; set; }
    public string? Ip { get; set; }
    public string? UserAgent { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? Detalle { get; set; }
}
