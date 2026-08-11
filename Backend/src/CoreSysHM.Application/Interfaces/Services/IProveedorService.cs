using CoreSysHM.Application.Common.Wrappers;
using CoreSysHM.Application.DTOs.Common;
using CoreSysHM.Application.DTOs.Stock;

namespace CoreSysHM.Application.Interfaces.Services;

public interface IProveedorService
{
    Task<ApiResponse<IEnumerable<ProveedorDto>>> GetAllAsync();
    Task<ApiResponse<ProveedorDto>> GetByIdAsync(int id);
    Task<ApiResponse<ProveedorDto>> CreateAsync(CreateProveedorDto dto, int? usuarioId = null);
    Task<ApiResponse<ProveedorDto>> UpdateAsync(int id, CreateProveedorDto dto, int? usuarioId = null);
    Task<ApiResponse<bool>> DeleteAsync(int id, int? usuarioId = null);
    Task<ApiResponse<IEnumerable<HistorialCambioDto>>> GetHistorialAsync(int id);
}
