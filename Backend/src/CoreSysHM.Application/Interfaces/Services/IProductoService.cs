using CoreSysHM.Application.Common.Wrappers;
using CoreSysHM.Application.DTOs.Stock;

namespace CoreSysHM.Application.Interfaces.Services;

public interface IProductoService
{
    Task<ApiResponse<IEnumerable<ProductoDto>>> GetAllAsync();
    Task<ApiResponse<ProductoDto>> GetByIdAsync(int id);
    Task<ApiResponse<ProductoDto>> CreateAsync(CreateProductoDto dto);
    Task<ApiResponse<ProductoDto>> UpdateAsync(int id, UpdateProductoDto dto);
    Task<ApiResponse<bool>> DeleteAsync(int id);
    Task<ApiResponse<IEnumerable<ProductoDto>>> GetStockBajoAsync();
    Task<ApiResponse<bool>> AjustarStockAsync(int id, int cantidad, string tipoMovimiento, string? observacion);
}
