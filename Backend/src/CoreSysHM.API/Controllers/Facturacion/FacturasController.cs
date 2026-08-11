using System.Security.Claims;
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
    [HasPermission(Permissions.Facturas.Create)]
    public async Task<IActionResult> EmitirFactura([FromBody] CreateFacturaDto dto)
    {
        var result = await _facturaService.EmitirFacturaAsync(dto, ObtenerUsuarioIdActual());
        return result.Exitoso ? CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result) : BadRequest(result);
    }

    [HttpPost("{id}/pagar")]
    [HasPermission(Permissions.Facturas.Create)]
    public async Task<IActionResult> MarcarPagada(int id)
    {
        var result = await _facturaService.MarcarPagadaAsync(id);
        return result.Exitoso ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/anular")]
    [HasPermission(Permissions.Facturas.Anular)]
    public async Task<IActionResult> Anular(int id)
    {
        var result = await _facturaService.AnularAsync(id, ObtenerUsuarioIdActual());
        return result.Exitoso ? Ok(result) : BadRequest(result);
    }

    [HttpGet("vencidas")]
    [HasPermission(Permissions.Facturas.View)]
    public async Task<IActionResult> GetVencidas()
    {
        var result = await _facturaService.GetVencidasAsync();
        return Ok(result);
    }

    /// <summary>Ventas Confirmadas con saldo pendiente de facturar (para armar "Nueva Factura").</summary>
    [HttpGet("ventas-facturables")]
    [HasPermission(Permissions.Facturas.Create)]
    public async Task<IActionResult> GetVentasFacturables([FromQuery] int? clienteId)
    {
        var result = await _facturaService.GetVentasFacturablesAsync(clienteId);
        return Ok(result);
    }

    /// <summary>Saldo pendiente de facturar, línea a línea, de una Venta puntual.</summary>
    [HttpGet("ventas/{ventaId}/saldo")]
    [HasPermission(Permissions.Facturas.Create)]
    public async Task<IActionResult> GetSaldoFacturar(int ventaId)
    {
        var result = await _facturaService.GetSaldoFacturarAsync(ventaId);
        return result.Exitoso ? Ok(result) : NotFound(result);
    }

    [HttpGet("tipos-comprobante")]
    [HasPermission(Permissions.Facturas.View)]
    public async Task<IActionResult> GetTiposComprobante()
    {
        var result = await _facturaService.GetTiposComprobanteAsync();
        return Ok(result);
    }

    [HttpGet("puntos-venta")]
    [HasPermission(Permissions.Facturas.View)]
    public async Task<IActionResult> GetPuntosVenta()
    {
        var result = await _facturaService.GetPuntosVentaAsync();
        return Ok(result);
    }

    private int? ObtenerUsuarioIdActual()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return int.TryParse(claim, out var id) ? id : null;
    }
}
