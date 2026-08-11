using System.Globalization;
using Microsoft.EntityFrameworkCore;
using CoreSysHM.Application.Common.Wrappers;
using CoreSysHM.Application.DTOs.Facturacion;
using CoreSysHM.Application.DTOs.Reportes;
using CoreSysHM.Application.Interfaces.Services;
using CoreSysHM.Domain.Enums;
using CoreSysHM.Infrastructure.Data;

namespace CoreSysHM.Infrastructure.Services;

public class ReporteFacturacionService : IReporteFacturacionService
{
    private readonly ApplicationDbContext _context;
    private readonly IFacturaService _facturaService;

    public ReporteFacturacionService(ApplicationDbContext context, IFacturaService facturaService)
    {
        _context = context;
        _facturaService = facturaService;
    }

    // ── 1. Facturación por período ────────────────────────────────────────────
    public async Task<ApiResponse<IEnumerable<FacturacionPorPeriodoDto>>> FacturacionPorPeriodoAsync(
        DateTime desde, DateTime hasta, string granularidad, int? puntoVentaId, int? tipoComprobanteId)
    {
        var hasta24 = hasta.Date.AddDays(1).AddTicks(-1);

        var query = _context.Facturas
            .Where(f => f.Activo
                     && f.Estado != EstadoFactura.Anulada
                     && f.FechaEmision >= desde
                     && f.FechaEmision <= hasta24);

        if (puntoVentaId.HasValue) query = query.Where(f => f.PuntoVentaId == puntoVentaId.Value);
        if (tipoComprobanteId.HasValue) query = query.Where(f => f.TipoComprobanteId == tipoComprobanteId.Value);

        var facturas = await query
            .Select(f => new { f.FechaEmision, f.Subtotal, f.Iva, f.Total })
            .ToListAsync();

        IEnumerable<FacturacionPorPeriodoDto> agrupadas = granularidad.ToLower() switch
        {
            "dia" => facturas
                .GroupBy(f => f.FechaEmision.ToString("yyyy-MM-dd"))
                .Select(g => new FacturacionPorPeriodoDto
                {
                    Periodo = g.Key, CantidadComprobantes = g.Count(),
                    Neto = g.Sum(f => f.Subtotal), Iva = g.Sum(f => f.Iva), Total = g.Sum(f => f.Total)
                }),

            "semana" => facturas
                .GroupBy(f => $"{ISOWeek.GetYear(f.FechaEmision)}-S{ISOWeek.GetWeekOfYear(f.FechaEmision):D2}")
                .Select(g => new FacturacionPorPeriodoDto
                {
                    Periodo = g.Key, CantidadComprobantes = g.Count(),
                    Neto = g.Sum(f => f.Subtotal), Iva = g.Sum(f => f.Iva), Total = g.Sum(f => f.Total)
                }),

            "año" or "anio" => facturas
                .GroupBy(f => f.FechaEmision.Year.ToString())
                .Select(g => new FacturacionPorPeriodoDto
                {
                    Periodo = g.Key, CantidadComprobantes = g.Count(),
                    Neto = g.Sum(f => f.Subtotal), Iva = g.Sum(f => f.Iva), Total = g.Sum(f => f.Total)
                }),

            _ => facturas // default: mes
                .GroupBy(f => f.FechaEmision.ToString("yyyy-MM"))
                .Select(g => new FacturacionPorPeriodoDto
                {
                    Periodo = g.Key, CantidadComprobantes = g.Count(),
                    Neto = g.Sum(f => f.Subtotal), Iva = g.Sum(f => f.Iva), Total = g.Sum(f => f.Total)
                })
        };

        return ApiResponse<IEnumerable<FacturacionPorPeriodoDto>>.Success(agrupadas.OrderBy(x => x.Periodo).ToList());
    }

    // ── 2. Desempeño por cliente ───────────────────────────────────────────────
    public async Task<ApiResponse<IEnumerable<DesempenoClienteDto>>> DesempenoPorClienteAsync(
        DateTime desde, DateTime hasta, int topN, int? clienteId)
    {
        var hasta24 = hasta.Date.AddDays(1).AddTicks(-1);

        var query = _context.Facturas
            .Where(f => f.Activo
                     && f.Estado != EstadoFactura.Anulada
                     && f.FechaEmision >= desde
                     && f.FechaEmision <= hasta24);

        if (clienteId.HasValue) query = query.Where(f => f.ClienteId == clienteId.Value);

        var filas = await query
            .GroupBy(f => new { f.ClienteId, Nombre = f.Cliente.Nombre + " " + f.Cliente.Apellido })
            .Select(g => new DesempenoClienteDto
            {
                ClienteId = g.Key.ClienteId,
                ClienteNombre = g.Key.Nombre,
                CantidadComprobantes = g.Count(),
                MontoTotal = g.Sum(f => f.Total),
                TicketPromedio = g.Average(f => f.Total)
            })
            .OrderByDescending(x => x.MontoTotal)
            .Take(topN)
            .ToListAsync();

        return ApiResponse<IEnumerable<DesempenoClienteDto>>.Success(filas);
    }

    // ── 3. Desempeño por producto ──────────────────────────────────────────────
    public async Task<ApiResponse<IEnumerable<DesempenoProductoDto>>> DesempenoPorProductoAsync(
        DateTime desde, DateTime hasta, int topN, int? productoId)
    {
        var hasta24 = hasta.Date.AddDays(1).AddTicks(-1);

        var query = _context.DetallesFactura
            .Where(d => d.Factura.Activo
                     && d.Factura.Estado != EstadoFactura.Anulada
                     && d.Factura.FechaEmision >= desde
                     && d.Factura.FechaEmision <= hasta24);

        if (productoId.HasValue) query = query.Where(d => d.ProductoId == productoId.Value);

        var filas = await query
            .GroupBy(d => new { d.ProductoId, d.Producto.Codigo, d.Producto.Nombre })
            .Select(g => new DesempenoProductoDto
            {
                ProductoId = g.Key.ProductoId,
                Codigo = g.Key.Codigo,
                Nombre = g.Key.Nombre,
                CantidadFacturada = g.Sum(d => d.Cantidad),
                MontoTotal = g.Sum(d => d.Subtotal)
            })
            .OrderByDescending(x => x.MontoTotal)
            .Take(topN)
            .ToListAsync();

        return ApiResponse<IEnumerable<DesempenoProductoDto>>.Success(filas);
    }

    // ── 4. Cartera por facturar ────────────────────────────────────────────────
    public async Task<ApiResponse<IEnumerable<VentaFacturableDto>>> CarteraPorFacturarAsync(int? clienteId)
        => await _facturaService.GetVentasFacturablesAsync(clienteId);
}
