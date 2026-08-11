using CoreSysHM.Application.Common.Wrappers;
using CoreSysHM.Application.DTOs.Common;
using CoreSysHM.Application.DTOs.Ventas;

namespace CoreSysHM.Application.Interfaces.Services;

public interface IClienteService
{
    Task<ApiResponse<IEnumerable<ClienteDto>>> GetAllAsync();
    Task<ApiResponse<ClienteDto>> GetByIdAsync(int id);
    Task<ApiResponse<ClienteDto>> CreateAsync(CreateClienteDto dto, int? usuarioId);
    Task<ApiResponse<ClienteDto>> UpdateAsync(int id, CreateClienteDto dto, int? usuarioId);
    Task<ApiResponse<bool>> DeleteAsync(int id, int? usuarioId);
    Task<ApiResponse<IEnumerable<HistorialCambioDto>>> GetHistorialAsync(int id);
}
