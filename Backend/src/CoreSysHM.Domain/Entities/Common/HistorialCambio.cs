using CoreSysHM.Domain.Entities.Auth;

namespace CoreSysHM.Domain.Entities.Common;

/// <summary>
/// Registro de auditoría de cambios sobre entidades de negocio (Cliente, Proveedor, Factura):
/// quién hizo qué y cuándo. Deliberadamente NO hereda de BaseEntity -- es un log de solo
/// escritura/lectura, sin baja lógica ni edición posterior (mismo criterio que AuditoriaAcceso).
/// </summary>
public class HistorialCambio
{
    public long Id { get; set; }

    /// <summary>Nombre de la entidad afectada, ej. "Cliente", "Proveedor", "Factura".</summary>
    public string Entidad { get; set; } = string.Empty;

    public int EntidadId { get; set; }

    /// <summary>Ej. "Alta", "Modificacion", "BajaLogica", "Emision", "Anulacion".</summary>
    public string Accion { get; set; } = string.Empty;

    public int? UsuarioId { get; set; }
    public ApplicationUser? Usuario { get; set; }

    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    /// <summary>Resumen legible de qué cambió (campos afectados, valores anteriores/nuevos si aplica).</summary>
    public string? Detalle { get; set; }
}
