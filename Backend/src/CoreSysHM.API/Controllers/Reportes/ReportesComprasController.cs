using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CoreSysHM.API.Security;
using CoreSysHM.Application.Interfaces.Services;
using CoreSysHM.Domain.Security;

namespace CoreSysHM.API.Controllers.Reportes;

[ApiController]
[Route("api/reportes/compras")]
[Authorize]
[HasPermission(Permissions.Reportes.Ver)]
public class ReportesComprasController : ControllerBase
{
    private readonly IReporteComprasService _svc;

    public ReportesComprasController(IReporteComprasService svc) => _svc = svc;

    /// <summary>
    /// Compras agrupadas por período (dia | semana | mes | año).
    /// GET api/reportes/compras/por-periodo?desde=2026-01-01&amp;hasta=2026-12-31&amp;granularidad=mes
    /// </summary>
    [HttpGet("por-periodo")]
    public async Task<IActionResult> ComprasPorPeriodo(
        [FromQuery] DateTime desde,
        [FromQuery] DateTime hasta,
        [FromQuery] string granularidad = "mes")
    {
        if (desde > hasta)
            return BadRequest("La fecha 'desde' no puede ser posterior a 'hasta'.");

        var result = await _svc.ComprasPorPeriodoAsync(desde, hasta, granularidad);
        return Ok(result);
    }

    /// <summary>
    /// Ranking de proveedores por monto total comprado.
    /// GET api/reportes/compras/ranking-proveedores?desde=2026-01-01&amp;hasta=2026-12-31&amp;topN=10
    /// </summary>
    [HttpGet("ranking-proveedores")]
    public async Task<IActionResult> RankingProveedores(
        [FromQuery] DateTime desde,
        [FromQuery] DateTime hasta,
        [FromQuery] int topN = 10)
    {
        if (desde > hasta)
            return BadRequest("La fecha 'desde' no puede ser posterior a 'hasta'.");
        if (topN <= 0)
            return BadRequest("'topN' debe ser mayor a cero.");

        var result = await _svc.RankingProveedoresAsync(desde, hasta, topN);
        return Ok(result);
    }

    /// <summary>
    /// Ranking de productos más comprados por cantidad o por monto.
    /// GET api/reportes/compras/productos-mas-comprados?desde=2026-01-01&amp;hasta=2026-12-31&amp;topN=10&amp;ordenarPor=cantidad
    /// </summary>
    [HttpGet("productos-mas-comprados")]
    public async Task<IActionResult> ProductosMasComprados(
        [FromQuery] DateTime desde,
        [FromQuery] DateTime hasta,
        [FromQuery] int topN = 10,
        [FromQuery] string ordenarPor = "cantidad")
    {
        if (desde > hasta)
            return BadRequest("La fecha 'desde' no puede ser posterior a 'hasta'.");
        if (topN <= 0)
            return BadRequest("'topN' debe ser mayor a cero.");

        var result = await _svc.ProductosMasCompradosAsync(desde, hasta, topN, ordenarPor);
        return Ok(result);
    }

    /// <summary>
    /// Evolución del precio unitario de compra para un producto a lo largo del tiempo.
    /// GET api/reportes/compras/evolucion-precio?productoId=1&amp;desde=2026-01-01&amp;hasta=2026-12-31
    /// </summary>
    [HttpGet("evolucion-precio")]
    public async Task<IActionResult> EvolucionPrecio(
        [FromQuery] int? productoId,
        [FromQuery] DateTime desde,
        [FromQuery] DateTime hasta)
    {
        if (productoId is null or <= 0)
            return BadRequest("'productoId' es obligatorio y debe ser mayor a cero.");
        if (desde > hasta)
            return BadRequest("La fecha 'desde' no puede ser posterior a 'hasta'.");

        var result = await _svc.EvolucionPrecioCompraAsync(productoId.Value, desde, hasta);
        return Ok(result);
    }

    /// <summary>
    /// Productos con stockActual &lt;= stockMinimo que requieren reposición.
    /// GET api/reportes/compras/sugerencias-reposicion?proveedorId=1
    /// </summary>
    [HttpGet("sugerencias-reposicion")]
    public async Task<IActionResult> SugerenciasReposicion(
        [FromQuery] int? proveedorId = null)
    {
        var result = await _svc.SugerenciasReposicionAsync(proveedorId);
        return Ok(result);
    }
}
