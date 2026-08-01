using CoreSysHM.Application.Common.Wrappers;
using CoreSysHM.Application.DTOs.Usuarios;

namespace CoreSysHM.Application.Interfaces.Services;

public interface IUserManagementService
{
    Task<ApiResponse<IEnumerable<UsuarioDto>>> GetAllAsync(string? rol = null, bool? activo = null);
    Task<ApiResponse<UsuarioDto>> GetByIdAsync(int id);
    Task<ApiResponse<UsuarioDto>> CreateAsync(CreateUsuarioDto dto);
    Task<ApiResponse<UsuarioDto>> UpdateAsync(int id, UpdateUsuarioDto dto);
    Task<ApiResponse<bool>> ToggleActivoAsync(int id, bool activo);
    Task<ApiResponse<bool>> ResetPasswordAsync(int id, string nuevaPassword);
    Task<ApiResponse<bool>> CambiarPasswordPropioAsync(int usuarioId, string passwordActual, string passwordNueva);
}
