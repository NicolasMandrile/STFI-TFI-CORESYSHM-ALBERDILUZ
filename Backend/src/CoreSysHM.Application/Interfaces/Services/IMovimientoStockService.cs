using CoreSysHM.Application.Common.Wrappers;
using CoreSysHM.Application.DTOs.Stock;

namespace CoreSysHM.Application.Interfaces.Services;

public interface IMovimientoStockService
{
    Task<ApiResponse<IEnumerable<MovimientoStockDto>>> GetAllAsync();
    Task<ApiResponse<IEnumerable<MovimientoStockDto>>> GetByProductoAsync(int productoId);
    Task<ApiResponse<MovimientoStockDto>> RegistrarMovimientoAsync(CreateMovimientoStockDto dto);
}
