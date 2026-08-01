using System.Globalization;
using Microsoft.EntityFrameworkCore;
using CoreSysHM.Application.Common.Wrappers;
using CoreSysHM.Application.DTOs.Reportes;
using CoreSysHM.Application.Interfaces.Services;
using CoreSysHM.Domain.Enums;
using CoreSysHM.Infrastructure.Data;

namespace CoreSysHM.Infrastructure.Services;

public class ReporteService : IReporteService
{
    private readonly ApplicationDbContext _context;

    public ReporteService(ApplicationDbContext context)
    {
        _context = context;
    }

    // ── 1. Productos más vendidos ─────────────────────────────────────────────
    public async Task<ApiResponse<IEnumerable<ProductoMasVendidoDto>>> ProductosMasVendidosAsync(
        DateTime desde, DateTime hasta, int topN, string ordenarPor)
    {
        var hasta24 = hasta.Date.AddDays(1).AddTicks(-1);

        // Se trae con contexto de descuento para aplicar factor proporcional por línea
        var raw = await _context.DetallesVenta
            .Where(d => d.Venta.Activo
                     && d.Venta.Estado != EstadoVenta.Anulada
                     && d.Venta.Fecha >= desde
                     && d.Venta.Fecha <= hasta24)
            .Select(d => new
            {
                d.ProductoId,
                d.Producto.Codigo,
                d.Producto.Nombre,
                d.Cantidad,
                LineaSubtotal  = d.Subtotal,
                VentaSubtotal  = d.Venta.Subtotal,
                VentaTotal     = d.Venta.Total
            })
            .ToListAsync();

        var filas = raw
            .GroupBy(d => new { d.ProductoId, d.Codigo, d.Nombre })
            .Select(g => new ProductoMasVendidoDto
            {
                ProductoId      = g.Key.ProductoId,
                Codigo          = g.Key.Codigo,
                Nombre          = g.Key.Nombre,
                CantidadVendida = g.Sum(d => d.Cantidad),
                TotalFacturado  = Math.Round(g.Sum(d =>
                    d.VentaSubtotal > 0
                        ? d.LineaSubtotal * (d.VentaTotal / d.VentaSubtotal)
                        : d.LineaSubtotal), 2)
            })
            .ToList();

        var ordenado = ordenarPor.ToLower() == "monto"
            ? filas.OrderByDescending(x => x.TotalFacturado)
            : filas.OrderByDescending(x => x.CantidadVendida);

        return ApiResponse<IEnumerable<ProductoMasVendidoDto>>
            .Success(ordenado.Take(topN));
    }

    // ── 2. Ventas por período ─────────────────────────────────────────────────
    public async Task<ApiResponse<IEnumerable<VentasPorPeriodoDto>>> VentasPorPeriodoAsync(
        DateTime desde, DateTime hasta, string granularidad)
    {
        var hasta24 = hasta.Date.AddDays(1).AddTicks(-1);

        var ventas = await _context.Ventas
            .Where(v => v.Activo
                     && v.Estado != EstadoVenta.Anulada
                     && v.Fecha >= desde
                     && v.Fecha <= hasta24)
            .Select(v => new { v.Fecha, v.Total })
            .ToListAsync();

        IEnumerable<VentasPorPeriodoDto> agrupadas = granularidad.ToLower() switch
        {
            "dia" => ventas
                .GroupBy(v => v.Fecha.ToString("yyyy-MM-dd"))
                .Select(g => new VentasPorPeriodoDto
                {
                    Periodo        = g.Key,
                    CantidadVentas = g.Count(),
                    TotalFacturado = g.Sum(v => v.Total)
                }),

            "semana" => ventas
                .GroupBy(v => $"{ISOWeek.GetYear(v.Fecha)}-S{ISOWeek.GetWeekOfYear(v.Fecha):D2}")
                .Select(g => new VentasPorPeriodoDto
                {
                    Periodo        = g.Key,
                    CantidadVentas = g.Count(),
                    TotalFacturado = g.Sum(v => v.Total)
                }),

            "año" or "anio" => ventas
                .GroupBy(v => v.Fecha.Year.ToString())
                .Select(g => new VentasPorPeriodoDto
                {
                    Periodo        = g.Key,
                    CantidadVentas = g.Count(),
                    TotalFacturado = g.Sum(v => v.Total)
                }),

            _ => ventas   // default: mes
                .GroupBy(v => v.Fecha.ToString("yyyy-MM"))
                .Select(g => new VentasPorPeriodoDto
                {
                    Periodo        = g.Key,
                    CantidadVentas = g.Count(),
                    TotalFacturado = g.Sum(v => v.Total)
                })
        };

        return ApiResponse<IEnumerable<VentasPorPeriodoDto>>
            .Success(agrupadas.OrderBy(x => x.Periodo).ToList());
    }

    // ── 3. Ranking de clientes ────────────────────────────────────────────────
    public async Task<ApiResponse<IEnumerable<RankingClienteDto>>> RankingClientesAsync(
        DateTime desde, DateTime hasta, int topN)
    {
        var hasta24 = hasta.Date.AddDays(1).AddTicks(-1);

        var filas = await _context.Ventas
            .Where(v => v.Activo
                     && v.Estado != EstadoVenta.Anulada
                     && v.Fecha >= desde
                     && v.Fecha <= hasta24)
            .GroupBy(v => new { v.ClienteId, v.Cliente.Nombre, v.Cliente.Apellido })
            .Select(g => new RankingClienteDto
            {
                ClienteId       = g.Key.ClienteId,
                Nombre          = g.Key.Nombre,
                Apellido        = g.Key.Apellido,
                CantidadCompras = g.Count(),
                MontoTotal      = g.Sum(v => v.Total),
                TicketPromedio  = g.Average(v => v.Total)
            })
            .OrderByDescending(x => x.MontoTotal)
            .Take(topN)
            .ToListAsync();

        return ApiResponse<IEnumerable<RankingClienteDto>>.Success(filas);
    }

    // ── 4. Ticket promedio ────────────────────────────────────────────────────
    public async Task<ApiResponse<TicketPromedioDto>> TicketPromedioAsync(
        DateTime desde, DateTime hasta)
    {
        var hasta24 = hasta.Date.AddDays(1).AddTicks(-1);

        var totales = await _context.Ventas
            .Where(v => v.Activo
                     && v.Estado != EstadoVenta.Anulada
                     && v.Fecha >= desde
                     && v.Fecha <= hasta24)
            .Select(v => v.Total)
            .ToListAsync();

        var cantidad = totales.Count;
        var total    = totales.Sum();

        return ApiResponse<TicketPromedioDto>.Success(new TicketPromedioDto
        {
            CantidadVentas = cantidad,
            TotalFacturado = total,
            TicketPromedio = cantidad > 0 ? Math.Round(total / cantidad, 2) : 0
        });
    }

    // ── 5. Margen / rentabilidad por producto ─────────────────────────────────
    public async Task<ApiResponse<IEnumerable<MargenProductoDto>>> MargenProductosAsync(
        DateTime desde, DateTime hasta, int topN)
    {
        var hasta24 = hasta.Date.AddDays(1).AddTicks(-1);

        // Factor de descuento aplicado proporcionalmente por línea; PrecioCompra es el actual (no histórico)
        var raw = await _context.DetallesVenta
            .Where(d => d.Venta.Activo
                     && d.Venta.Estado != EstadoVenta.Anulada
                     && d.Venta.Fecha >= desde
                     && d.Venta.Fecha <= hasta24)
            .Select(d => new
            {
                d.ProductoId,
                d.Producto.Codigo,
                d.Producto.Nombre,
                d.Producto.PrecioCompra,
                d.Cantidad,
                LineaSubtotal = d.Subtotal,
                VentaSubtotal = d.Venta.Subtotal,
                VentaTotal    = d.Venta.Total
            })
            .ToListAsync();

        var filas = raw
            .GroupBy(d => new { d.ProductoId, d.Codigo, d.Nombre, d.PrecioCompra })
            .Select(g => new
            {
                g.Key.ProductoId,
                g.Key.Codigo,
                g.Key.Nombre,
                g.Key.PrecioCompra,
                CantidadVendida = g.Sum(d => d.Cantidad),
                IngresoTotal    = Math.Round(g.Sum(d =>
                    d.VentaSubtotal > 0
                        ? d.LineaSubtotal * (d.VentaTotal / d.VentaSubtotal)
                        : d.LineaSubtotal), 2)
            })
            .ToList();

        var resultado = filas.Select(f =>
        {
            var costoTotal  = f.PrecioCompra * f.CantidadVendida;
            var margenTotal = f.IngresoTotal - costoTotal;
            return new MargenProductoDto
            {
                ProductoId       = f.ProductoId,
                Codigo           = f.Codigo,
                Nombre           = f.Nombre,
                CantidadVendida  = f.CantidadVendida,
                IngresoTotal     = f.IngresoTotal,
                CostoTotal       = costoTotal,
                MargenTotal      = margenTotal,
                MargenPorcentual = f.IngresoTotal > 0
                    ? Math.Round(margenTotal / f.IngresoTotal * 100, 2)
                    : 0
            };
        })
        .OrderByDescending(x => x.MargenTotal)
        .Take(topN);

        return ApiResponse<IEnumerable<MargenProductoDto>>.Success(resultado.ToList());
    }
}
