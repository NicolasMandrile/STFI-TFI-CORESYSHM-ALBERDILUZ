using CoreSysHM.Application.Common.Wrappers;
using CoreSysHM.Application.DTOs.Compras;

namespace CoreSysHM.Application.Interfaces.Services;

public interface ICompraService
{
    Task<ApiResponse<IEnumerable<CompraDto>>> GetAllAsync();
    Task<ApiResponse<CompraDto>> GetByIdAsync(int id);
    Task<ApiResponse<CompraDto>> CreateAsync(CreateCompraDto dto);
    Task<ApiResponse<bool>> AnularAsync(int id);
    Task<ApiResponse<IEnumerable<CompraDto>>> GetByProveedorAsync(int proveedorId);
}
