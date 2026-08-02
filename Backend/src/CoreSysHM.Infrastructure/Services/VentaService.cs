using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CoreSysHM.Application.Common.Wrappers;
using CoreSysHM.Application.DTOs.Ventas;
using CoreSysHM.Application.Interfaces.Services;
using CoreSysHM.Domain.Entities.Stock;
using CoreSysHM.Domain.Entities.Ventas;
using CoreSysHM.Domain.Enums;
using CoreSysHM.Domain.Interfaces;
using CoreSysHM.Infrastructure.Data;

namespace CoreSysHM.Infrastructure.Services;

public class VentaService : IVentaService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ApplicationDbContext _context;

    public VentaService(IUnitOfWork uow, IMapper mapper, ApplicationDbContext context)
    {
        _uow = uow;
        _mapper = mapper;
        _context = context;
    }

    public async Task<ApiResponse<IEnumerable<VentaDto>>> GetAllAsync(int? clienteIdFiltro = null)
    {
        var query = _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Detalles).ThenInclude(d => d.Producto)
            .Where(v => v.Activo);

        if (clienteIdFiltro.HasValue)
            query = query.Where(v => v.ClienteId == clienteIdFiltro.Value);

        var ventas = await query.OrderByDescending(v => v.Fecha).ToListAsync();
        return ApiResponse<IEnumerable<VentaDto>>.Success(_mapper.Map<IEnumerable<VentaDto>>(ventas));
    }

    public async Task<ApiResponse<VentaDto>> GetByIdAsync(int id, int? clienteIdFiltro = null)
    {
        var venta = await _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Detalles).ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(v => v.Id == id && v.Activo
                && (!clienteIdFiltro.HasValue || v.ClienteId == clienteIdFiltro.Value));

        if (venta is null)
            return ApiResponse<VentaDto>.Failure("Venta no encontrada.");

        return ApiResponse<VentaDto>.Success(_mapper.Map<VentaDto>(venta));
    }

    public async Task<int?> GetClienteIdByUserIdAsync(int userId)
    {
        return await _context.Clientes
            .Where(c => c.UserId == userId && c.Activo)
            .Select(c => (int?)c.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<ApiResponse<VentaDto>> CreateAsync(CreateVentaDto dto, bool creadaPorCliente = false)
    {
        if (!dto.Detalles.Any())
            return ApiResponse<VentaDto>.Failure("La venta debe tener al menos un producto.");

        var cliente = await _context.Clientes.FindAsync(dto.ClienteId);
        if (cliente is null || !cliente.Activo)
            return ApiResponse<VentaDto>.Failure("Cliente no encontrado.");

        if (string.IsNullOrWhiteSpace(cliente.Direccion) || string.IsNullOrWhiteSpace(cliente.Localidad))
            return ApiResponse<VentaDto>.Failure("El cliente debe tener dirección y localidad completas para registrar una venta.");

        // Cargar y validar productos
        var productosIds = dto.Detalles.Select(d => d.ProductoId).Distinct().ToList();
        var productos = await _context.Productos
            .Where(p => productosIds.Contains(p.Id) && p.Activo)
            .ToDictionaryAsync(p => p.Id);

        foreach (var det in dto.Detalles)
        {
            if (det.Cantidad <= 0)
                return ApiResponse<VentaDto>.Failure($"La cantidad para el producto {det.ProductoId} debe ser mayor a cero.");
            if (!productos.ContainsKey(det.ProductoId))
                return ApiResponse<VentaDto>.Failure($"Producto con ID {det.ProductoId} no encontrado.");
            if (productos[det.ProductoId].StockActual < det.Cantidad)
                return ApiResponse<VentaDto>.Failure($"Stock insuficiente para '{productos[det.ProductoId].Nombre}'. Disponible: {productos[det.ProductoId].StockActual}.");
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var numero = $"V-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(1000, 9999)}";
            var venta = new Venta
            {
                NumeroVenta = numero,
                Fecha = DateTime.UtcNow,
                ClienteId = dto.ClienteId,
                Descuento = dto.Descuento,
                Estado = creadaPorCliente ? EstadoVenta.Pendiente : EstadoVenta.Confirmada,
                Observaciones = dto.Observaciones
            };

            decimal subtotal = 0;
            var movimientos = new List<MovimientoStock>();

            foreach (var det in dto.Detalles)
            {
                var producto = productos[det.ProductoId];
                var precio = producto.PrecioVenta;
                var linea = new DetalleVenta
                {
                    ProductoId = det.ProductoId,
                    Cantidad = det.Cantidad,
                    PrecioUnitario = precio,
                    Subtotal = precio * det.Cantidad
                };
                venta.Detalles.Add(linea);
                subtotal += linea.Subtotal;

                // Descontar stock
                var stockAnterior = producto.StockActual;
                producto.StockActual -= det.Cantidad;
                _context.Productos.Update(producto);

                movimientos.Add(new MovimientoStock
                {
                    ProductoId = det.ProductoId,
                    Cantidad = det.Cantidad,
                    TipoMovimiento = "VENTA",
                    Observacion = $"Venta {numero}",
                    StockAnterior = stockAnterior,
                    StockPosterior = producto.StockActual
                });
            }

            venta.Subtotal = subtotal;
            venta.Total = subtotal - dto.Descuento;

            await _context.Ventas.AddAsync(venta);
            await _context.MovimientosStock.AddRangeAsync(movimientos);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            await _context.Entry(venta).Reference(v => v.Cliente).LoadAsync();
            return ApiResponse<VentaDto>.Success(_mapper.Map<VentaDto>(venta), "Venta registrada correctamente.");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<ApiResponse<bool>> ConfirmarAsync(int id)
    {
        var venta = await _context.Ventas.FindAsync(id);
        if (venta is null || !venta.Activo)
            return ApiResponse<bool>.Failure("Venta no encontrada.");
        if (venta.Estado == EstadoVenta.Confirmada)
            return ApiResponse<bool>.Failure("La venta ya está confirmada.");

        venta.Estado = EstadoVenta.Confirmada;
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Success(true, "Venta confirmada.");
    }

    public async Task<ApiResponse<bool>> AnularAsync(int id)
    {
        var venta = await _context.Ventas
            .Include(v => v.Detalles).ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(v => v.Id == id && v.Activo);

        if (venta is null)
            return ApiResponse<bool>.Failure("Venta no encontrada.");
        if (venta.Estado == EstadoVenta.Anulada)
            return ApiResponse<bool>.Failure("La venta ya está anulada.");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var movimientos = new List<MovimientoStock>();
            foreach (var det in venta.Detalles)
            {
                var producto = det.Producto;
                var stockAnterior = producto.StockActual;
                producto.StockActual += det.Cantidad;
                _context.Productos.Update(producto);

                movimientos.Add(new MovimientoStock
                {
                    ProductoId = det.ProductoId,
                    Cantidad = det.Cantidad,
                    TipoMovimiento = "ANULACION_VENTA",
                    Observacion = $"Anulación de {venta.NumeroVenta}",
                    StockAnterior = stockAnterior,
                    StockPosterior = producto.StockActual
                });
            }

            venta.Estado = EstadoVenta.Anulada;
            await _context.MovimientosStock.AddRangeAsync(movimientos);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return ApiResponse<bool>.Success(true, "Venta anulada y stock revertido.");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<ApiResponse<IEnumerable<ClienteDto>>> GetClientesAsync()
    {
        var clientes = await _context.Clientes.Where(c => c.Activo).OrderBy(c => c.Apellido).ToListAsync();
        return ApiResponse<IEnumerable<ClienteDto>>.Success(_mapper.Map<IEnumerable<ClienteDto>>(clientes));
    }

    public async Task<ApiResponse<ClienteDto>> CreateClienteAsync(CreateClienteDto dto)
    {
        var cliente = _mapper.Map<Cliente>(dto);
        await _context.Clientes.AddAsync(cliente);
        await _context.SaveChangesAsync();
        return ApiResponse<ClienteDto>.Success(_mapper.Map<ClienteDto>(cliente), "Cliente creado correctamente.");
    }
}
