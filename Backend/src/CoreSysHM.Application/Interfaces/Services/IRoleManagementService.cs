using CoreSysHM.Application.Common.Wrappers;
using CoreSysHM.Application.DTOs.Roles;

namespace CoreSysHM.Application.Interfaces.Services;

public interface IRoleManagementService
{
    Task<ApiResponse<IEnumerable<RoleDto>>> GetAllAsync();
    Task<ApiResponse<RoleDto>> GetByIdAsync(int id);
    Task<ApiResponse<RoleDto>> CreateAsync(CreateRoleDto dto);
    Task<ApiResponse<RoleDto>> UpdateAsync(int id, UpdateRoleDto dto);
    Task<ApiResponse<bool>> DeleteAsync(int id);
    Task<ApiResponse<IReadOnlyList<string>>> GetCatalogoPermisosAsync();
}
