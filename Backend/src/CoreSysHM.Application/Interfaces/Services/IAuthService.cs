using CoreSysHM.Application.Common.Wrappers;
using CoreSysHM.Application.DTOs.Auth;

namespace CoreSysHM.Application.Interfaces.Services;

public interface IAuthService
{
    Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto dto, string? ip, string? userAgent);
}
