using CoreSysHM.Application.Common.Wrappers;
using CoreSysHM.Application.DTOs.Auditoria;
using CoreSysHM.Domain.Enums;

namespace CoreSysHM.Application.Interfaces.Services;

/// <summary>
/// A propósito NO expone ningún método de borrado: el log de auditoría no es eliminable
/// ni siquiera vía soft-delete.
/// </summary>
public interface IAuditoriaService
{
    Task RegistrarAsync(int? usuarioId, string? rolSnapshot, TipoAccionAuditoria accion,
        string? ip, string? userAgent, string? detalle = null);

    Task<ApiResponse<PagedResponse<AuditoriaAccesoDto>>> BuscarAsync(AuditoriaFiltroDto filtro);
}
