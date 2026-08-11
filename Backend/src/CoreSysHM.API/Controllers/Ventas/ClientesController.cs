using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CoreSysHM.API.Security;
using CoreSysHM.Application.DTOs.Ventas;
using CoreSysHM.Application.Interfaces.Services;
using CoreSysHM.Domain.Security;

namespace CoreSysHM.API.Controllers.Ventas;

/// <summary>
/// CRUD completo de Clientes (maestro). GET/POST también existen como acciones más livianas en
/// VentasController (api/ventas/clientes) para el flujo de autoservicio de "Nueva Venta"; este
/// controller es el de gestión del maestro (editar, baja, historial de cambios).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClientesController : ControllerBase
{
    private readonly IClienteService _clienteService;

    public ClientesController(IClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    [HttpGet]
    [HasPermission(Permissions.Clientes.View)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _clienteService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    [HasPermission(Permissions.Clientes.View)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _clienteService.GetByIdAsync(id);
        return result.Exitoso ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [HasPermission(Permissions.Clientes.Create)]
    public async Task<IActionResult> Create([FromBody] CreateClienteDto dto)
    {
        var result = await _clienteService.CreateAsync(dto, ObtenerUsuarioIdActual());
        return result.Exitoso ? CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result) : BadRequest(result);
    }

    [HttpPut("{id}")]
    [HasPermission(Permissions.Clientes.Edit)]
    public async Task<IActionResult> Update(int id, [FromBody] CreateClienteDto dto)
    {
        var result = await _clienteService.UpdateAsync(id, dto, ObtenerUsuarioIdActual());
        return result.Exitoso ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    [HasPermission(Permissions.Clientes.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _clienteService.DeleteAsync(id, ObtenerUsuarioIdActual());
        return result.Exitoso ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id}/historial")]
    [HasPermission(Permissions.Clientes.View)]
    public async Task<IActionResult> GetHistorial(int id)
    {
        var result = await _clienteService.GetHistorialAsync(id);
        return Ok(result);
    }

    private int? ObtenerUsuarioIdActual()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return int.TryParse(claim, out var id) ? id : null;
    }
}
