using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CoreSysHM.API.Security;
using CoreSysHM.Application.Interfaces.Services;
using CoreSysHM.Domain.Security;

namespace CoreSysHM.API.Controllers.Reportes;

[ApiController]
[Route("api/reportes/facturacion")]
[Authorize]
[HasPermission(Permissions.Reportes.Ver)]
public class ReportesFacturacionController : ControllerBase
{
    private readonly IReporteFacturacionService _svc;

    public ReportesFacturacionController(IReporteFacturacionService svc) => _svc = svc;

    /// <summary>
    /// Facturación agrupada por período (dia | semana | mes | año), con neto/IVA/total desglosados.
    /// GET api/reportes/facturacion/por-periodo?desde=2026-01-01&amp;hasta=2026-12-31&amp;granularidad=mes
    /// </summary>
    [HttpGet("por-periodo")]
    public async Task<IActionResult> PorPeriodo(
        [FromQuery] DateTime desde,
        [FromQuery] DateTime hasta,
        [FromQuery] string granularidad = "mes",
        [FromQuery] int? puntoVentaId = null,
        [FromQuery] int? tipoComprobanteId = null)
    {
        if (desde > hasta)
            return BadRequest("La fecha 'desde' no puede ser posterior a 'hasta'.");

        var result = await _svc.FacturacionPorPeriodoAsync(desde, hasta, granularidad, puntoVentaId, tipoComprobanteId);
        return Ok(result);
    }

    /// <summary>
    /// Desempeño de facturación por cliente (monto, cantidad, ticket promedio).
    /// GET api/reportes/facturacion/desempeno-clientes?desde=2026-01-01&amp;hasta=2026-12-31&amp;topN=10
    /// </summary>
    [HttpGet("desempeno-clientes")]
    public async Task<IActionResult> DesempenoClientes(
        [FromQuery] DateTime desde,
        [FromQuery] DateTime hasta,
        [FromQuery] int topN = 10,
        [FromQuery] int? clienteId = null)
    {
        if (desde > hasta)
            return BadRequest("La fecha 'desde' no puede ser posterior a 'hasta'.");
        if (topN <= 0)
            return BadRequest("'topN' debe ser mayor a cero.");

        var result = await _svc.DesempenoPorClienteAsync(desde, hasta, topN, clienteId);
        return Ok(result);
    }

    /// <summary>
    /// Desempeño de facturación por producto (cantidad y monto facturado).
    /// GET api/reportes/facturacion/desempeno-productos?desde=2026-01-01&amp;hasta=2026-12-31&amp;topN=10
    /// </summary>
    [HttpGet("desempeno-productos")]
    public async Task<IActionResult> DesempenoProductos(
        [FromQuery] DateTime desde,
        [FromQuery] DateTime hasta,
        [FromQuery] int topN = 10,
        [FromQuery] int? productoId = null)
    {
        if (desde > hasta)
            return BadRequest("La fecha 'desde' no puede ser posterior a 'hasta'.");
        if (topN <= 0)
            return BadRequest("'topN' debe ser mayor a cero.");

        var result = await _svc.DesempenoPorProductoAsync(desde, hasta, topN, productoId);
        return Ok(result);
    }

    /// <summary>
    /// Ventas confirmadas con saldo pendiente de facturar (total o parcial).
    /// GET api/reportes/facturacion/cartera-por-facturar?clienteId=1
    /// </summary>
    [HttpGet("cartera-por-facturar")]
    public async Task<IActionResult> CarteraPorFacturar([FromQuery] int? clienteId = null)
    {
        var result = await _svc.CarteraPorFacturarAsync(clienteId);
        return Ok(result);
    }
}
