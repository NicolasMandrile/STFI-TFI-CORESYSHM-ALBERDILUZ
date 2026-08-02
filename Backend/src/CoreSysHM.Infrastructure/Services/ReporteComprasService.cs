using System.Globalization;
using Microsoft.EntityFrameworkCore;
using CoreSysHM.Application.Common.Wrappers;
using CoreSysHM.Application.DTOs.Reportes;
using CoreSysHM.Application.Interfaces.Services;
using CoreSysHM.Infrastructure.Data;

namespace CoreSysHM.Infrastructure.Services;

public class ReporteComprasService : IReporteComprasService
{
    private const int EstadoConfirmada = 1;

    private readonly ApplicationDbContext _context;

    public ReporteComprasService(ApplicationDbContext context)
    {
        _context = context;
    }

    // ── 1. Compras por período ────────────────────────────────────────────────
    public async Task<ApiResponse<IEnumerable<ComprasPorPeriodoDto>>> ComprasPorPeriodoAsync(
        DateTime desde, DateTime hasta, string granularidad)
    {
        var hasta24 = hasta.Date.AddDays(1).AddTicks(-1);

        var compras = await _context.Compras
            .Where(c => c.Activo
                     && c.EstadoCompraId == EstadoConfirmada
                     && c.Fecha >= desde
                     && c.Fecha <= hasta24)
            .Select(c => new { c.Fecha, c.Total })
            .ToListAsync();

        IEnumerable<ComprasPorPeriodoDto> agrupadas = granularidad.ToLower() switch
        {
            "dia" => compras
                .GroupBy(c => c.Fecha.ToString("yyyy-MM-dd"))
                .Select(g => new ComprasPorPeriodoDto
                {
                    Periodo         = g.Key,
                    CantidadCompras = g.Count(),
                    TotalGastado    = g.Sum(c => c.Total)
                }),

            "semana" => compras
                .GroupBy(c => $"{ISOWeek.GetYear(c.Fecha)}-S{ISOWeek.GetWeekOfYear(c.Fecha):D2}")
                .Select(g => new ComprasPorPeriodoDto
                {
                    Periodo         = g.Key,
                    CantidadCompras = g.Count(),
                    TotalGastado    = g.Sum(c => c.Total)
                }),

            "año" or "anio" => compras
                .GroupBy(c => c.Fecha.Year.ToString())
                .Select(g => new ComprasPorPeriodoDto
                {
                    Periodo         = g.Key,
                    CantidadCompras = g.Count(),
                    TotalGastado    = g.Sum(c => c.Total)
                }),

            _ => compras  // default: mes
                .GroupBy(c => c.Fecha.ToString("yyyy-MM"))
                .Select(g => new ComprasPorPeriodoDto
                {
                    Periodo         = g.Key,
                    CantidadCompras = g.Count(),
                    TotalGastado    = g.Sum(c => c.Total)
                })
        };

        return ApiResponse<IEnumerable<ComprasPorPeriodoDto>>
            .Success(agrupadas.OrderBy(x => x.Periodo).ToList());
    }

    // ── 2. Ranking de proveedores ─────────────────────────────────────────────
    public async Task<ApiResponse<IEnumerable<RankingProveedorDto>>> RankingProveedoresAsync(
        DateTime desde, DateTime hasta, int topN)
    {
        var hasta24 = hasta.Date.AddDays(1).AddTicks(-1);

        var filas = await _context.Compras
            .Where(c => c.Activo
                     && c.EstadoCompraId == EstadoConfirmada
                     && c.Fecha >= desde
                     && c.Fecha <= hasta24)
            .GroupBy(c => new { c.ProveedorId, c.Proveedor.RazonSocial, c.Proveedor.Cuit })
            .Select(g => new RankingProveedorDto
            {
                ProveedorId     = g.Key.ProveedorId,
                RazonSocial     = g.Key.RazonSocial,
                Cuit            = g.Key.Cuit,
                CantidadCompras = g.Count(),
                MontoTotal      = g.Sum(c => c.Total),
                TicketPromedio  = g.Average(c => c.Total)
            })
            .OrderByDescending(x => x.MontoTotal)
            .Take(topN)
            .ToListAsync();

        return ApiResponse<IEnumerable<RankingProveedorDto>>.Success(filas);
    }

    // ── 3. Productos más comprados ────────────────────────────────────────────
    public async Task<ApiResponse<IEnumerable<ProductoMasCompradoDto>>> ProductosMasCompradosAsync(
        DateTime desde, DateTime hasta, int topN, string ordenarPor)
    {
        var hasta24 = hasta.Date.AddDays(1).AddTicks(-1);

        var filas = await _context.DetallesCompra
            .Where(d => d.Compra.Activo
                     && d.Compra.EstadoCompraId == EstadoConfirmada
                     && d.Compra.Fecha >= desde
                     && d.Compra.Fecha <= hasta24)
            .GroupBy(d => new { d.ProductoId, d.Producto.Codigo, d.Producto.Nombre })
            .Select(g => new ProductoMasCompradoDto
            {
                ProductoId       = g.Key.ProductoId,
                Codigo           = g.Key.Codigo,
                Nombre           = g.Key.Nombre,
                CantidadComprada = g.Sum(d => d.Cantidad),
                MontoTotal       = g.Sum(d => d.Subtotal)
            })
            .ToListAsync();

        var ordenado = ordenarPor.ToLower() == "monto"
            ? filas.OrderByDescending(x => x.MontoTotal)
            : filas.OrderByDescending(x => x.CantidadComprada);

        return ApiResponse<IEnumerable<ProductoMasCompradoDto>>
            .Success(ordenado.Take(topN).ToList());
    }

    // ── 4. Evolución del precio de compra por producto ─────────────────────────
    public async Task<ApiResponse<IEnumerable<EvolucionPrecioCompraDto>>> EvolucionPrecioCompraAsync(
        int productoId, DateTime desde, DateTime hasta)
    {
        var hasta24 = hasta.Date.AddDays(1).AddTicks(-1);

        var registros = await _context.DetallesCompra
            .Where(d => d.ProductoId == productoId
                     && d.Compra.Activo
                     && d.Compra.EstadoCompraId == EstadoConfirmada
                     && d.Compra.Fecha >= desde
                     && d.Compra.Fecha <= hasta24)
            .Select(d => new { d.Compra.Fecha, d.PrecioUnitario, d.Compra.NumeroCompra })
            .ToListAsync();

        // DateTime.ToString(format) no es traducible a SQL: se formatea y ordena en memoria
        // una vez materializados los registros (mismo patrón que ComprasPorPeriodoAsync).
        var filas = registros
            .OrderBy(r => r.Fecha)
            .Select(r => new EvolucionPrecioCompraDto
            {
                Fecha          = r.Fecha.ToString("yyyy-MM-dd"),
                PrecioUnitario = r.PrecioUnitario,
                NumeroCompra   = r.NumeroCompra
            })
            .ToList();

        return ApiResponse<IEnumerable<EvolucionPrecioCompraDto>>.Success(filas);
    }

    // ── 5. Sugerencia de reposición ───────────────────────────────────────────
    public async Task<ApiResponse<IEnumerable<SugerenciaReposicionDto>>> SugerenciasReposicionAsync(
        int? proveedorId)
    {
        // Productos cuyo stockActual <= stockMinimo
        var query = _context.Productos
            .Where(p => p.Activo && p.StockActual <= p.StockMinimo);

        if (proveedorId.HasValue)
            query = query.Where(p => p.ProveedorId == proveedorId.Value);

        var productos = await query
            .Select(p => new
            {
                p.Id,
                p.Codigo,
                p.Nombre,
                p.StockActual,
                p.StockMinimo,
                p.ProveedorId,
                ProveedorNombre = p.Proveedor != null ? p.Proveedor.RazonSocial : string.Empty
            })
            .ToListAsync();

        // Último precio de compra por producto (de compras confirmadas)
        var productoIds = productos.Select(p => p.Id).ToList();

        var ultimosPreciosRaw = await _context.DetallesCompra
            .Where(d => productoIds.Contains(d.ProductoId)
                     && d.Compra.Activo
                     && d.Compra.EstadoCompraId == EstadoConfirmada)
            .GroupBy(d => d.ProductoId)
            .Select(g => new
            {
                ProductoId = g.Key,
                UltimoPrecio = g.OrderByDescending(d => d.Compra.Fecha).First().PrecioUnitario
            })
            .ToListAsync();

        var ultimosPrecios = ultimosPreciosRaw.ToDictionary(x => x.ProductoId, x => x.UltimoPrecio);

        var resultado = productos.Select(p => new SugerenciaReposicionDto
        {
            ProductoId         = p.Id,
            Codigo             = p.Codigo,
            Nombre             = p.Nombre,
            StockActual        = p.StockActual,
            StockMinimo        = p.StockMinimo,
            Diferencia         = p.StockMinimo - p.StockActual,
            ProveedorNombre    = p.ProveedorNombre,
            UltimoPrecioCompra = ultimosPrecios.TryGetValue(p.Id, out var precio) ? precio : 0m
        })
        .OrderByDescending(x => x.Diferencia)
        .ToList();

        return ApiResponse<IEnumerable<SugerenciaReposicionDto>>.Success(resultado);
    }
}
