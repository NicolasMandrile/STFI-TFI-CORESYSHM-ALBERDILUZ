using CoreSysHM.Application.Common.Wrappers;
using CoreSysHM.Application.DTOs.Facturacion;

namespace CoreSysHM.Application.Interfaces.Services;

public interface IFacturaService
{
    Task<ApiResponse<IEnumerable<FacturaDto>>> GetAllAsync();
    Task<ApiResponse<FacturaDto>> GetByIdAsync(int id);
    Task<ApiResponse<FacturaDto>> EmitirFacturaAsync(CreateFacturaDto dto);
    Task<ApiResponse<bool>> MarcarPagadaAsync(int id);
    Task<ApiResponse<bool>> AnularAsync(int id);
    Task<ApiResponse<IEnumerable<FacturaDto>>> GetVencidasAsync();
}
