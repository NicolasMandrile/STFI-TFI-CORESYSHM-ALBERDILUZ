using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CoreSysHM.API.Security;
using CoreSysHM.Application.DTOs.Facturacion;
using CoreSysHM.Application.Interfaces.Services;
using CoreSysHM.Domain.Security;

namespace CoreSysHM.API.Controllers.Facturacion;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FacturasController : ControllerBase
{
    private readonly IFacturaService _facturaService;

    public FacturasController(IFacturaService facturaService)
    {
        _facturaService = facturaService;
    }

    [HttpGet]
    [HasPermission(Permissions.Facturas.View)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _facturaService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    [HasPermission(Permissions.Facturas.View)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _facturaService.GetByIdAsync(id);
        return result.Exitoso ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador,Administrativo")]
    public async Task<IActionResult> EmitirFactura([FromBody] CreateFacturaDto dto)
    {
        var result = await _facturaService.EmitirFacturaAsync(dto);
        return result.Exitoso ? CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result) : BadRequest(result);
    }

    [HttpPost("{id}/pagar")]
    [Authorize(Roles = "Administrador,Administrativo")]
    public async Task<IActionResult> MarcarPagada(int id)
    {
        var result = await _facturaService.MarcarPagadaAsync(id);
        return result.Exitoso ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/anular")]
    [Authorize(Roles = "Administrador,Administrativo")]
    public async Task<IActionResult> Anular(int id)
    {
        var result = await _facturaService.AnularAsync(id);
        return result.Exitoso ? Ok(result) : BadRequest(result);
    }

    [HttpGet("vencidas")]
    [HasPermission(Permissions.Facturas.View)]
    public async Task<IActionResult> GetVencidas()
    {
        var result = await _facturaService.GetVencidasAsync();
        return Ok(result);
    }
}
