using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using CoreSysHM.Application.Common.Wrappers;
using CoreSysHM.Application.DTOs.Facturacion;
using CoreSysHM.Application.Interfaces.Services;
using CoreSysHM.Domain.Entities.Facturacion;
using CoreSysHM.Domain.Entities.Stock;
using CoreSysHM.Domain.Enums;
using CoreSysHM.Infrastructure.Data;

namespace CoreSysHM.Infrastructure.Services;

public class FacturaService : IFacturaService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IHistorialCambioService _historial;

    public FacturaService(ApplicationDbContext context, IMapper mapper, IHistorialCambioService historial)
    {
        _context = context;
        _mapper = mapper;
        _historial = historial;
    }

    private IQueryable<Factura> ConIncludes(IQueryable<Factura> query) =>
        query.Include(f => f.Cliente)
             .Include(f => f.Venta)
             .Include(f => f.TipoComprobante)
             .Include(f => f.PuntoVenta)
             .Include(f => f.Detalles).ThenInclude(d => d.Producto);

    public async Task<ApiResponse<IEnumerable<FacturaDto>>> GetAllAsync()
    {
        var facturas = await ConIncludes(_context.Facturas.Where(f => f.Activo))
            .OrderByDescending(f => f.FechaEmision)
            .ToListAsync();
        return ApiResponse<IEnumerable<FacturaDto>>.Success(_mapper.Map<IEnumerable<FacturaDto>>(facturas));
    }

    public async Task<ApiResponse<FacturaDto>> GetByIdAsync(int id)
    {
        var factura = await ConIncludes(_context.Facturas).FirstOrDefaultAsync(f => f.Id == id && f.Activo);
        if (factura is null)
            return ApiResponse<FacturaDto>.Failure("Factura no encontrada.");
        return ApiResponse<FacturaDto>.Success(_mapper.Map<FacturaDto>(factura));
    }

    public async Task<ApiResponse<IEnumerable<FacturaDto>>> GetVencidasAsync()
    {
        var vencidas = await ConIncludes(_context.Facturas.Where(f => f.Activo))
            .Where(f => f.FechaVencimiento != null && f.FechaVencimiento < DateTime.UtcNow && f.Estado == EstadoFactura.Emitida)
            .OrderBy(f => f.FechaVencimiento)
            .ToListAsync();
        return ApiResponse<IEnumerable<FacturaDto>>.Success(_mapper.Map<IEnumerable<FacturaDto>>(vencidas));
    }

    // ── Saldo pendiente de facturar ─────────────────────────────────────────────
    // Única fuente de verdad: la usan tanto "Nueva Factura" (frontend) como el reporte de
    // Cartera por facturar (Bloque 4), así que ambos siempre coinciden por construcción.
    public async Task<ApiResponse<IEnumerable<VentaFacturableDto>>> GetVentasFacturablesAsync(int? clienteId = null)
    {
        var query = _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Detalles).ThenInclude(d => d.Producto)
            .Where(v => v.Activo && v.Estado == EstadoVenta.Confirmada);

        if (clienteId.HasValue)
            query = query.Where(v => v.ClienteId == clienteId.Value);

        var ventas = await query.OrderByDescending(v => v.Fecha).ToListAsync();
        if (ventas.Count == 0)
            return ApiResponse<IEnumerable<VentaFacturableDto>>.Success(Enumerable.Empty<VentaFacturableDto>());

        var ventaIds = ventas.Select(v => v.Id).ToList();
        var facturadoPorLinea = await FacturadoPorLineaAsync(ventaIds);

        var resultado = ventas
            .Select(v => ArmarVentaFacturable(v, facturadoPorLinea))
            .Where(vf => vf.Lineas.Any(l => l.CantidadPendiente > 0))
            .ToList();

        return ApiResponse<IEnumerable<VentaFacturableDto>>.Success(resultado);
    }

    public async Task<ApiResponse<VentaFacturableDto>> GetSaldoFacturarAsync(int ventaId)
    {
        var venta = await _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Detalles).ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(v => v.Id == ventaId && v.Activo);
        if (venta is null)
            return ApiResponse<VentaFacturableDto>.Failure("Venta no encontrada.");

        var facturadoPorLinea = await FacturadoPorLineaAsync(new[] { ventaId });
        return ApiResponse<VentaFacturableDto>.Success(ArmarVentaFacturable(venta, facturadoPorLinea));
    }

    private async Task<Dictionary<int, int>> FacturadoPorLineaAsync(IEnumerable<int> ventaIds)
    {
        var detalleVentaIds = await _context.DetallesVenta
            .Where(d => ventaIds.Contains(d.VentaId))
            .Select(d => d.Id)
            .ToListAsync();

        return await _context.DetallesFactura
            .Where(df => detalleVentaIds.Contains(df.DetalleVentaId) && df.Factura.Estado != EstadoFactura.Anulada)
            .GroupBy(df => df.DetalleVentaId)
            .Select(g => new { DetalleVentaId = g.Key, Cantidad = g.Sum(x => x.Cantidad) })
            .ToDictionaryAsync(x => x.DetalleVentaId, x => x.Cantidad);
    }

    private static VentaFacturableDto ArmarVentaFacturable(Domain.Entities.Ventas.Venta venta, Dictionary<int, int> facturadoPorLinea) =>
        new()
        {
            VentaId = venta.Id,
            NumeroVenta = venta.NumeroVenta,
            ClienteId = venta.ClienteId,
            ClienteNombre = venta.Cliente != null ? $"{venta.Cliente.Nombre} {venta.Cliente.Apellido}" : string.Empty,
            Fecha = venta.Fecha,
            Lineas = venta.Detalles.Select(d =>
            {
                var facturado = facturadoPorLinea.TryGetValue(d.Id, out var c) ? c : 0;
                return new SaldoFacturarLineaDto
                {
                    DetalleVentaId = d.Id,
                    ProductoId = d.ProductoId,
                    ProductoNombre = d.Producto.Nombre,
                    PrecioUnitario = d.PrecioUnitario,
                    CantidadVenta = d.Cantidad,
                    CantidadFacturada = facturado,
                    CantidadPendiente = d.Cantidad - facturado
                };
            }).ToList()
        };

    public async Task<ApiResponse<IEnumerable<TipoComprobanteDto>>> GetTiposComprobanteAsync()
    {
        var tipos = await _context.TiposComprobante
            .Where(t => t.Activo)
            .OrderBy(t => t.Descripcion)
            .Select(t => new TipoComprobanteDto { Id = t.Id, Descripcion = t.Descripcion, AfectaStock = t.AfectaStock, SignoContable = t.SignoContable })
            .ToListAsync();
        return ApiResponse<IEnumerable<TipoComprobanteDto>>.Success(tipos);
    }

    public async Task<ApiResponse<IEnumerable<PuntoVentaDto>>> GetPuntosVentaAsync()
    {
        var puntos = await _context.PuntosVenta
            .Where(p => p.Activo)
            .OrderBy(p => p.Descripcion)
            .Select(p => new PuntoVentaDto { Id = p.Id, Descripcion = p.Descripcion })
            .ToListAsync();
        return ApiResponse<IEnumerable<PuntoVentaDto>>.Success(puntos);
    }

    // ── Emisión ──────────────────────────────────────────────────────────────────
    public async Task<ApiResponse<FacturaDto>> EmitirFacturaAsync(CreateFacturaDto dto, int? usuarioId)
    {
        if (string.IsNullOrWhiteSpace(dto.IdempotencyKey))
            return ApiResponse<FacturaDto>.Failure("IdempotencyKey es obligatoria.");

        var existente = await ConIncludes(_context.Facturas).FirstOrDefaultAsync(f => f.IdempotencyKey == dto.IdempotencyKey);
        if (existente is not null)
            return ApiResponse<FacturaDto>.Success(_mapper.Map<FacturaDto>(existente), "Factura ya emitida (idempotente).");

        if (!dto.Detalles.Any())
            return ApiResponse<FacturaDto>.Failure("La factura debe tener al menos una línea.");

        var venta = await _context.Ventas.Include(v => v.Cliente).FirstOrDefaultAsync(v => v.Id == dto.VentaId && v.Activo);
        if (venta is null)
            return ApiResponse<FacturaDto>.Failure("Venta no encontrada.");
        if (venta.Estado != EstadoVenta.Confirmada)
            return ApiResponse<FacturaDto>.Failure("Solo se pueden facturar ventas confirmadas.");

        var tipoComprobante = await _context.TiposComprobante.FirstOrDefaultAsync(t => t.Id == dto.TipoComprobanteId && t.Activo);
        if (tipoComprobante is null)
            return ApiResponse<FacturaDto>.Failure("Tipo de comprobante no encontrado.");

        var puntoVenta = await _context.PuntosVenta.FirstOrDefaultAsync(p => p.Id == dto.PuntoVentaId && p.Activo);
        if (puntoVenta is null)
            return ApiResponse<FacturaDto>.Failure("Punto de venta no encontrado.");

        var detalleVentaIds = dto.Detalles.Select(d => d.DetalleVentaId).Distinct().ToList();
        var detallesVenta = await _context.DetallesVenta
            .Include(d => d.Producto)
            .Where(d => detalleVentaIds.Contains(d.Id) && d.VentaId == dto.VentaId)
            .ToDictionaryAsync(d => d.Id);

        var facturadoPorLinea = await FacturadoPorLineaAsync(new[] { dto.VentaId });

        decimal subtotal = 0, ivaTotal = 0;
        var detallesFactura = new List<DetalleFactura>();
        var movimientos = new List<MovimientoStock>();

        foreach (var lineaDto in dto.Detalles)
        {
            if (lineaDto.Cantidad <= 0)
                return ApiResponse<FacturaDto>.Failure("La cantidad de cada línea debe ser mayor a cero.");
            if (!detallesVenta.TryGetValue(lineaDto.DetalleVentaId, out var detalleVenta))
                return ApiResponse<FacturaDto>.Failure($"La línea {lineaDto.DetalleVentaId} no pertenece a la venta indicada.");

            var facturadoPrevio = facturadoPorLinea.TryGetValue(lineaDto.DetalleVentaId, out var c) ? c : 0;
            var pendiente = detalleVenta.Cantidad - facturadoPrevio;
            if (lineaDto.Cantidad > pendiente)
                return ApiResponse<FacturaDto>.Failure(
                    $"Cantidad a facturar ({lineaDto.Cantidad}) supera el saldo pendiente ({pendiente}) para '{detalleVenta.Producto.Nombre}'.");

            var neto = (detalleVenta.PrecioUnitario * lineaDto.Cantidad) - lineaDto.Descuento;
            var ivaLinea = Math.Round(neto * (lineaDto.Impuesto / 100m), 2);
            subtotal += neto;
            ivaTotal += ivaLinea;

            detallesFactura.Add(new DetalleFactura
            {
                ProductoId = detalleVenta.ProductoId,
                DetalleVentaId = detalleVenta.Id,
                Cantidad = lineaDto.Cantidad,
                PrecioUnitario = detalleVenta.PrecioUnitario,
                Impuesto = lineaDto.Impuesto,
                Descuento = lineaDto.Descuento,
                Subtotal = neto
            });

            // El stock ya se descontó al confirmar la Venta (VentaService.CreateAsync) -- la
            // Factura documenta fiscalmente una venta cuyo stock ya se movió, no lo vuelve a
            // mover. Solo las Notas de Crédito (signo "-") generan un movimiento propio, porque
            // reponen stock por una devolución que todavía no fue registrada en ningún lado.
            if (tipoComprobante.AfectaStock && tipoComprobante.SignoContable == "-")
            {
                var producto = detalleVenta.Producto;
                var stockAnterior = producto.StockActual;
                producto.StockActual += lineaDto.Cantidad;
                movimientos.Add(new MovimientoStock
                {
                    ProductoId = producto.Id,
                    Cantidad = lineaDto.Cantidad,
                    TipoMovimiento = "ENTRADA",
                    Observacion = $"{tipoComprobante.Descripcion} (a emitir sobre {venta.NumeroVenta})",
                    StockAnterior = stockAnterior,
                    StockPosterior = producto.StockActual
                });
            }
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var numero = await AsignarNumeroAsync(dto.PuntoVentaId, dto.TipoComprobanteId, puntoVenta.Descripcion);

            var factura = new Factura
            {
                NumeroFactura = numero,
                FechaEmision = DateTime.UtcNow,
                FechaVencimiento = dto.FechaVencimiento,
                ClienteId = venta.ClienteId,
                VentaId = venta.Id,
                TipoComprobanteId = dto.TipoComprobanteId,
                PuntoVentaId = dto.PuntoVentaId,
                Subtotal = subtotal,
                Iva = ivaTotal,
                Total = subtotal + ivaTotal,
                Estado = EstadoFactura.Emitida,
                Observaciones = dto.Observaciones,
                IdempotencyKey = dto.IdempotencyKey,
                Detalles = detallesFactura
            };

            await _context.Facturas.AddAsync(factura);
            if (movimientos.Count > 0)
                await _context.MovimientosStock.AddRangeAsync(movimientos);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            await _historial.RegistrarAsync("Factura", factura.Id, "Emision", usuarioId,
                $"Factura {factura.NumeroFactura} emitida por ${factura.Total:N2} sobre venta {venta.NumeroVenta}.");

            var creada = await GetByIdAsync(factura.Id);
            return ApiResponse<FacturaDto>.Success(creada.Data!, "Factura emitida correctamente.");
        }
        catch (DbUpdateException ex) when (EsViolacionDeUnicidad(ex))
        {
            // Carrera entre dos requests con la misma IdempotencyKey: la otra ganó la inserción;
            // se devuelve la factura que efectivamente quedó grabada, en vez de fallar.
            await transaction.RollbackAsync();
            var ganadora = await ConIncludes(_context.Facturas).FirstOrDefaultAsync(f => f.IdempotencyKey == dto.IdempotencyKey);
            if (ganadora is not null)
                return ApiResponse<FacturaDto>.Success(_mapper.Map<FacturaDto>(ganadora), "Factura ya emitida (idempotente).");
            throw;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // UPDATE atómico sobre el contador: SQL Server toma un lock de la fila durante el UPDATE,
    // así que dos emisiones concurrentes para el mismo (PuntoVenta, TipoComprobante) nunca
    // pueden leer el mismo UltimoNumero -- es lo que garantiza 0% de números duplicados.
    private async Task<string> AsignarNumeroAsync(int puntoVentaId, int tipoComprobanteId, string puntoVentaDescripcion)
    {
        int nuevoNumero;
        if (_context.Database.IsSqlServer())
        {
            // UPDATE ... OUTPUT: SQL Server toma un lock de la fila durante el UPDATE, así que dos
            // emisiones concurrentes para el mismo (PuntoVenta, TipoComprobante) nunca pueden leer
            // el mismo UltimoNumero -- es lo que garantiza 0% de números duplicados.
            // No es SQL "componible" -- .ToListAsync() lo materializa tal cual en vez de intentar
            // envolverlo en un SELECT (que es lo que hace SingleAsync() directo sobre el
            // IQueryable de SqlQueryRaw, y rompe contra un UPDATE).
            var resultados = await _context.Database.SqlQueryRaw<int>(
                "UPDATE NumeracionesComprobante SET UltimoNumero = UltimoNumero + 1 " +
                "OUTPUT INSERTED.UltimoNumero " +
                "WHERE PuntoVentaId = {0} AND TipoComprobanteId = {1}",
                puntoVentaId, tipoComprobanteId).ToListAsync();
            nuevoNumero = resultados.Single();
        }
        else
        {
            // Sqlite (tests): no soporta OUTPUT/RETURNING vía SqlQueryRaw en esta versión de EF.
            // Los tests no ejercen concurrencia real entre conexiones, así que un incremento
            // leído-y-guardado por EF alcanza para validar la numeración correlativa.
            var contador = await _context.NumeracionesComprobante
                .SingleAsync(n => n.PuntoVentaId == puntoVentaId && n.TipoComprobanteId == tipoComprobanteId);
            contador.UltimoNumero++;
            await _context.SaveChangesAsync();
            nuevoNumero = contador.UltimoNumero;
        }

        return $"{puntoVentaDescripcion.PadLeft(4, '0')}-{nuevoNumero.ToString().PadLeft(8, '0')}";
    }

    private static bool EsViolacionDeUnicidad(DbUpdateException ex) =>
        ex.InnerException is SqlException sqlEx && (sqlEx.Number == 2601 || sqlEx.Number == 2627);

    // ── Anulación / pago ─────────────────────────────────────────────────────────
    public async Task<ApiResponse<bool>> MarcarPagadaAsync(int id)
    {
        var factura = await _context.Facturas.FirstOrDefaultAsync(f => f.Id == id && f.Activo);
        if (factura is null)
            return ApiResponse<bool>.Failure("Factura no encontrada.");
        if (factura.Estado != EstadoFactura.Emitida)
            return ApiResponse<bool>.Failure("Solo se puede marcar como pagada una factura Emitida.");

        factura.Estado = EstadoFactura.Pagada;
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Success(true, "Factura marcada como pagada.");
    }

    public async Task<ApiResponse<bool>> AnularAsync(int id, int? usuarioId)
    {
        var factura = await _context.Facturas
            .Include(f => f.TipoComprobante)
            .Include(f => f.Detalles).ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(f => f.Id == id && f.Activo);
        if (factura is null)
            return ApiResponse<bool>.Failure("Factura no encontrada.");
        if (factura.Estado == EstadoFactura.Anulada)
            return ApiResponse<bool>.Failure("La factura ya está anulada.");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Simétrico a la emisión: solo las Notas de Crédito tocaron stock al emitirse (repusieron
            // por una devolución), así que solo ellas revierten (vuelven a descontar) al anularse.
            // Anular una Factura normal (signo "+") no toca stock -- nunca lo había tocado.
            if (factura.TipoComprobante.AfectaStock && factura.TipoComprobante.SignoContable == "-")
            {
                var movimientos = new List<MovimientoStock>();
                foreach (var detalle in factura.Detalles)
                {
                    var producto = detalle.Producto;
                    var stockAnterior = producto.StockActual;
                    producto.StockActual -= detalle.Cantidad;
                    movimientos.Add(new MovimientoStock
                    {
                        ProductoId = producto.Id,
                        Cantidad = detalle.Cantidad,
                        TipoMovimiento = "SALIDA",
                        Observacion = $"Anulación de {factura.NumeroFactura}",
                        StockAnterior = stockAnterior,
                        StockPosterior = producto.StockActual
                    });
                }
                await _context.MovimientosStock.AddRangeAsync(movimientos);
            }

            var tocoStock = factura.TipoComprobante.AfectaStock && factura.TipoComprobante.SignoContable == "-";
            factura.Estado = EstadoFactura.Anulada;
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            await _historial.RegistrarAsync("Factura", factura.Id, "Anulacion", usuarioId, $"Factura {factura.NumeroFactura} anulada.");
            return ApiResponse<bool>.Success(true, tocoStock ? "Factura anulada y stock revertido." : "Factura anulada.");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
