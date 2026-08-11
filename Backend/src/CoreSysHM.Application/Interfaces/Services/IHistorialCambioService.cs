using CoreSysHM.Application.DTOs.Common;

namespace CoreSysHM.Application.Interfaces.Services;

/// <summary>
/// Auditoría de cambios sobre entidades de negocio (Cliente, Proveedor, Factura). A propósito
/// no expone ningún método de borrado -- mismo criterio que IAuditoriaService.
/// </summary>
public interface IHistorialCambioService
{
    Task RegistrarAsync(string entidad, int entidadId, string accion, int? usuarioId, string? detalle = null);

    Task<IEnumerable<HistorialCambioDto>> GetHistorialAsync(string entidad, int entidadId);
}
