using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CoreSysHM.API.Security;
using CoreSysHM.Application.DTOs.Usuarios;
using CoreSysHM.Application.Interfaces.Services;
using CoreSysHM.Domain.Security;

namespace CoreSysHM.API.Controllers.Usuarios;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserManagementService _userManagementService;

    public UsersController(IUserManagementService userManagementService)
    {
        _userManagementService = userManagementService;
    }

    [HttpGet]
    [HasPermission(Permissions.Usuarios.View)]
    public async Task<IActionResult> GetAll([FromQuery] string? rol, [FromQuery] bool? activo)
    {
        var result = await _userManagementService.GetAllAsync(rol, activo);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [HasPermission(Permissions.Usuarios.View)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _userManagementService.GetByIdAsync(id);
        return result.Exitoso ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [HasPermission(Permissions.Usuarios.Create)]
    public async Task<IActionResult> Create([FromBody] CreateUsuarioDto dto)
    {
        var result = await _userManagementService.CreateAsync(dto);
        return result.Exitoso ? CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result) : BadRequest(result);
    }

    [HttpPut("{id:int}")]
    [HasPermission(Permissions.Usuarios.Edit)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUsuarioDto dto)
    {
        var result = await _userManagementService.UpdateAsync(id, dto);
        return result.Exitoso ? Ok(result) : BadRequest(result);
    }

    [HttpPatch("{id:int}/activo")]
    [HasPermission(Permissions.Usuarios.Edit)]
    public async Task<IActionResult> ToggleActivo(int id, [FromQuery] bool activo)
    {
        var result = await _userManagementService.ToggleActivoAsync(id, activo);
        return result.Exitoso ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id:int}/reset-password")]
    [HasPermission(Permissions.Usuarios.ResetPassword)]
    public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordDto dto)
    {
        var result = await _userManagementService.ResetPasswordAsync(id, dto.NuevaPassword);
        return result.Exitoso ? Ok(result) : BadRequest(result);
    }

    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var id = ObtenerUsuarioIdActual();
        if (id is null) return Unauthorized();
        var result = await _userManagementService.GetByIdAsync(id.Value);
        return result.Exitoso ? Ok(result) : NotFound(result);
    }

    [HttpPost("me/cambiar-password")]
    public async Task<IActionResult> CambiarPasswordPropio([FromBody] CambiarPasswordPropioDto dto)
    {
        var id = ObtenerUsuarioIdActual();
        if (id is null) return Unauthorized();

        var result = await _userManagementService.CambiarPasswordPropioAsync(id.Value, dto.PasswordActual, dto.PasswordNueva);
        return result.Exitoso ? Ok(result) : BadRequest(result);
    }

    private int? ObtenerUsuarioIdActual()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return int.TryParse(claim, out var id) ? id : null;
    }
}
