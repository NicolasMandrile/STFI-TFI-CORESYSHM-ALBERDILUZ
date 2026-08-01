using CoreSysHM.Application.Common.Wrappers;
using CoreSysHM.Application.DTOs.Stock;

namespace CoreSysHM.Application.Interfaces.Services;

public interface ICategoriaService
{
    Task<ApiResponse<IEnumerable<CategoriaDto>>> GetAllAsync();
    Task<ApiResponse<CategoriaDto>> GetByIdAsync(int id);
    Task<ApiResponse<CategoriaDto>> CreateAsync(CreateCategoriaDto dto);
    Task<ApiResponse<CategoriaDto>> UpdateAsync(int id, CreateCategoriaDto dto);
    Task<ApiResponse<bool>> DeleteAsync(int id);
}
