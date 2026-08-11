using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CoreSysHM.API.Security;
using CoreSysHM.Application.DTOs.Stock;
using CoreSysHM.Application.Interfaces.Services;
using CoreSysHM.Domain.Security;

namespace CoreSysHM.API.Controllers.Stock;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProveedoresController : ControllerBase
{
    private readonly IProveedorService _proveedorService;

    public ProveedoresController(IProveedorService proveedorService)
    {
        _proveedorService = proveedorService;
    }

    [HttpGet]
    [HasPermission(Permissions.Proveedores.View)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _proveedorService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    [HasPermission(Permissions.Proveedores.View)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _proveedorService.GetByIdAsync(id);
        return result.Exitoso ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [HasPermission(Permissions.Proveedores.Create)]
    public async Task<IActionResult> Create([FromBody] CreateProveedorDto dto)
    {
        var result = await _proveedorService.CreateAsync(dto, ObtenerUsuarioIdActual());
        return result.Exitoso ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id}")]
    [HasPermission(Permissions.Proveedores.Edit)]
    public async Task<IActionResult> Update(int id, [FromBody] CreateProveedorDto dto)
    {
        var result = await _proveedorService.UpdateAsync(id, dto, ObtenerUsuarioIdActual());
        return result.Exitoso ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _proveedorService.DeleteAsync(id, ObtenerUsuarioIdActual());
        return result.Exitoso ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id}/historial")]
    [HasPermission(Permissions.Proveedores.View)]
    public async Task<IActionResult> GetHistorial(int id)
    {
        var result = await _proveedorService.GetHistorialAsync(id);
        return Ok(result);
    }

    private int? ObtenerUsuarioIdActual()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return int.TryParse(claim, out var id) ? id : null;
    }
}
