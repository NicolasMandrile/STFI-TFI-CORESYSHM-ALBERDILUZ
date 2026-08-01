using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CoreSysHM.API.Security;
using CoreSysHM.Application.Interfaces.Services;
using CoreSysHM.Domain.Security;

namespace CoreSysHM.API.Controllers.Reportes;

[ApiController]
[Route("api/reportes/ventas")]
[Authorize]
[HasPermission(Permissions.Reportes.Ver)]
public class ReportesController : ControllerBase
{
    private readonly IReporteService _svc;

    public ReportesController(IReporteService svc) => _svc = svc;

    /// <summary>
    /// Productos más vendidos — ranking por cantidad o por monto.
    /// GET api/reportes/ventas/productos-mas-vendidos?desde=2025-01-01&amp;hasta=2025-12-31&amp;topN=10&amp;ordenarPor=cantidad
    /// </summary>
    [HttpGet("productos-mas-vendidos")]
    public async Task<IActionResult> ProductosMasVendidos(
        [FromQuery] DateTime desde,
        [FromQuery] DateTime hasta,
        [FromQuery] int topN = 10,
        [FromQuery] string ordenarPor = "cantidad")
    {
        if (desde > hasta)
            return BadRequest("La fecha 'desde' no puede ser posterior a 'hasta'.");
        if (topN <= 0)
            return BadRequest("'topN' debe ser mayor a cero.");

        var result = await _svc.ProductosMasVendidosAsync(desde, hasta, topN, ordenarPor);
        return Ok(result);
    }

    /// <summary>
    /// Ventas agrupadas por período (dia | semana | mes | año).
    /// GET api/reportes/ventas/por-periodo?desde=2025-01-01&amp;hasta=2025-12-31&amp;granularidad=mes
    /// </summary>
    [HttpGet("por-periodo")]
    public async Task<IActionResult> VentasPorPeriodo(
        [FromQuery] DateTime desde,
        [FromQuery] DateTime hasta,
        [FromQuery] string granularidad = "mes")
    {
        if (desde > hasta)
            return BadRequest("La fecha 'desde' no puede ser posterior a 'hasta'.");

        var result = await _svc.VentasPorPeriodoAsync(desde, hasta, granularidad);
        return Ok(result);
    }

    /// <summary>
    /// Ranking de clientes por monto total comprado.
    /// GET api/reportes/ventas/ranking-clientes?desde=2025-01-01&amp;hasta=2025-12-31&amp;topN=10
    /// </summary>
    [HttpGet("ranking-clientes")]
    public async Task<IActionResult> RankingClientes(
        [FromQuery] DateTime desde,
        [FromQuery] DateTime hasta,
        [FromQuery] int topN = 10)
    {
        if (desde > hasta)
            return BadRequest("La fecha 'desde' no puede ser posterior a 'hasta'.");
        if (topN <= 0)
            return BadRequest("'topN' debe ser mayor a cero.");

        var result = await _svc.RankingClientesAsync(desde, hasta, topN);
        return Ok(result);
    }

    /// <summary>
    /// Ticket promedio de ventas confirmadas en el período.
    /// GET api/reportes/ventas/ticket-promedio?desde=2025-01-01&amp;hasta=2025-12-31
    /// </summary>
    [HttpGet("ticket-promedio")]
    public async Task<IActionResult> TicketPromedio(
        [FromQuery] DateTime desde,
        [FromQuery] DateTime hasta)
    {
        if (desde > hasta)
            return BadRequest("La fecha 'desde' no puede ser posterior a 'hasta'.");

        var result = await _svc.TicketPromedioAsync(desde, hasta);
        return Ok(result);
    }

    /// <summary>
    /// Margen / rentabilidad por producto.
    /// GET api/reportes/ventas/margen-productos?desde=2025-01-01&amp;hasta=2025-12-31&amp;topN=10
    /// </summary>
    [HttpGet("margen-productos")]
    public async Task<IActionResult> MargenProductos(
        [FromQuery] DateTime desde,
        [FromQuery] DateTime hasta,
        [FromQuery] int topN = 10)
    {
        if (desde > hasta)
            return BadRequest("La fecha 'desde' no puede ser posterior a 'hasta'.");
        if (topN <= 0)
            return BadRequest("'topN' debe ser mayor a cero.");

        var result = await _svc.MargenProductosAsync(desde, hasta, topN);
        return Ok(result);
    }
}
