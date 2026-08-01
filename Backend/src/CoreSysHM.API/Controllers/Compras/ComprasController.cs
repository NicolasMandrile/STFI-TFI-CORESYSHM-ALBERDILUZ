using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CoreSysHM.API.Security;
using CoreSysHM.Application.DTOs.Compras;
using CoreSysHM.Application.Interfaces.Services;
using CoreSysHM.Domain.Security;

namespace CoreSysHM.API.Controllers.Compras;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ComprasController : ControllerBase
{
    private readonly ICompraService _compraService;

    public ComprasController(ICompraService compraService)
    {
        _compraService = compraService;
    }

    [HttpGet]
    [HasPermission(Permissions.Compras.View)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _compraService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    [HasPermission(Permissions.Compras.View)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _compraService.GetByIdAsync(id);
        return result.Exitoso ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [HasPermission(Permissions.Compras.Create)]
    public async Task<IActionResult> Create([FromBody] CreateCompraDto dto)
    {
        var result = await _compraService.CreateAsync(dto);
        return result.Exitoso ? CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result) : BadRequest(result);
    }

    [HttpPost("{id}/anular")]
    [Authorize(Roles = "Administrador,Administrativo")]
    public async Task<IActionResult> Anular(int id)
    {
        var result = await _compraService.AnularAsync(id);
        return result.Exitoso ? Ok(result) : BadRequest(result);
    }

    [HttpGet("proveedor/{proveedorId}")]
    [HasPermission(Permissions.Compras.View)]
    public async Task<IActionResult> GetByProveedor(int proveedorId)
    {
        var result = await _compraService.GetByProveedorAsync(proveedorId);
        return Ok(result);
    }
}
