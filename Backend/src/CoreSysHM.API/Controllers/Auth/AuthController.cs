using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CoreSysHM.Application.DTOs.Auth;
using CoreSysHM.Application.Interfaces.Services;
using CoreSysHM.Domain.Enums;

namespace CoreSysHM.API.Controllers.Auth;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IAuditoriaService _auditoriaService;

    public AuthController(IAuthService authService, IAuditoriaService auditoriaService)
    {
        _authService = authService;
        _auditoriaService = auditoriaService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        var (ip, userAgent) = ObtenerContextoCliente();
        var result = await _authService.LoginAsync(dto, ip, userAgent);
        return result.Exitoso ? Ok(result) : Unauthorized(result);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var (ip, userAgent) = ObtenerContextoCliente();
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        var rol = User.FindFirstValue(ClaimTypes.Role);

        await _auditoriaService.RegistrarAsync(
            int.TryParse(userId, out var id) ? id : null,
            rol, TipoAccionAuditoria.Logout, ip, userAgent);

        return Ok();
    }

    private (string? Ip, string? UserAgent) ObtenerContextoCliente()
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();
        return (ip, string.IsNullOrWhiteSpace(userAgent) ? null : userAgent);
    }
}
