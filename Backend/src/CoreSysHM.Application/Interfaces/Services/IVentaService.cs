using CoreSysHM.Application.Common.Wrappers;
using CoreSysHM.Application.DTOs.Ventas;

namespace CoreSysHM.Application.Interfaces.Services;

public interface IVentaService
{
    /// <param name="clienteIdFiltro">
    /// Null = sin filtro (staff, ve todas). Con valor = solo ventas de ese Cliente
    /// (portal de cliente, ver VentasController.ObtenerFiltroClienteAsync).
    /// </param>
    Task<ApiResponse<IEnumerable<VentaDto>>> GetAllAsync(int? clienteIdFiltro = null);
    Task<ApiResponse<VentaDto>> GetByIdAsync(int id, int? clienteIdFiltro = null);
    Task<ApiResponse<VentaDto>> CreateAsync(CreateVentaDto dto);
    Task<ApiResponse<bool>> ConfirmarAsync(int id);
    Task<ApiResponse<bool>> AnularAsync(int id);
    Task<ApiResponse<IEnumerable<ClienteDto>>> GetClientesAsync();
    Task<ApiResponse<ClienteDto>> CreateClienteAsync(CreateClienteDto dto);

    /// <summary>Cliente de negocio vinculado a este login (rol Cliente), o null si no tiene vínculo.</summary>
    Task<int?> GetClienteIdByUserIdAsync(int userId);
}
