using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CoreSysHM.API.Security;
using CoreSysHM.Application.DTOs.Roles;
using CoreSysHM.Application.Interfaces.Services;
using CoreSysHM.Domain.Security;

namespace CoreSysHM.API.Controllers.Roles;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly IRoleManagementService _roleManagementService;

    public RolesController(IRoleManagementService roleManagementService)
    {
        _roleManagementService = roleManagementService;
    }

    [HttpGet]
    [HasPermission(Permissions.Seguridad.View)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _roleManagementService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [HasPermission(Permissions.Seguridad.View)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _roleManagementService.GetByIdAsync(id);
        return result.Exitoso ? Ok(result) : NotFound(result);
    }

    [HttpGet("catalogo-permisos")]
    [HasPermission(Permissions.Seguridad.View)]
    public async Task<IActionResult> GetCatalogoPermisos()
    {
        var result = await _roleManagementService.GetCatalogoPermisosAsync();
        return Ok(result);
    }

    [HttpPost]
    [HasPermission(Permissions.Seguridad.Manage)]
    public async Task<IActionResult> Create([FromBody] CreateRoleDto dto)
    {
        var result = await _roleManagementService.CreateAsync(dto);
        return result.Exitoso ? CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result) : BadRequest(result);
    }

    [HttpPut("{id:int}")]
    [HasPermission(Permissions.Seguridad.Manage)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRoleDto dto)
    {
        var result = await _roleManagementService.UpdateAsync(id, dto);
        return result.Exitoso ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:int}")]
    [HasPermission(Permissions.Seguridad.Manage)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _roleManagementService.DeleteAsync(id);
        return result.Exitoso ? Ok(result) : BadRequest(result);
    }
}
