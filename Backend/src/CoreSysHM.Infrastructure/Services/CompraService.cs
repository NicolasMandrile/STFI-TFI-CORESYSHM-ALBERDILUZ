using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CoreSysHM.Application.Common.Wrappers;
using CoreSysHM.Application.DTOs.Compras;
using CoreSysHM.Application.Interfaces.Services;
using CoreSysHM.Domain.Entities.Compras;
using CoreSysHM.Domain.Entities.Stock;
using CoreSysHM.Domain.Interfaces;
using CoreSysHM.Infrastructure.Data;

namespace CoreSysHM.Infrastructure.Services;

public class CompraService : ICompraService
{
    // IDs de EstadoCompra según seed (HasData en EstadoCompraConfiguration)
    private const int EstadoConfirmada = 1;
    private const int EstadoAnulada    = 2;

    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ApplicationDbContext _context;

    public CompraService(IUnitOfWork uow, IMapper mapper, ApplicationDbContext context)
    {
        _uow = uow;
        _mapper = mapper;
        _context = context;
    }

    public async Task<ApiResponse<IEnumerable<CompraDto>>> GetAllAsync()
    {
        var compras = await _context.Compras
            .Include(c => c.Proveedor)
            .Include(c => c.EstadoCompra)
            .Include(c => c.RegistradoPor)
            .Include(c => c.Detalles).ThenInclude(d => d.Producto)
            .Where(c => c.Activo)
            .OrderByDescending(c => c.Fecha)
            .ToListAsync();
        return ApiResponse<IEnumerable<CompraDto>>.Success(_mapper.Map<IEnumerable<CompraDto>>(compras));
    }

    public async Task<ApiResponse<CompraDto>> GetByIdAsync(int id)
    {
        var compra = await _context.Compras
            .Include(c => c.Proveedor)
            .Include(c => c.EstadoCompra)
            .Include(c => c.RegistradoPor)
            .Include(c => c.Detalles).ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(c => c.Id == id && c.Activo);
        if (compra is null)
            return ApiResponse<CompraDto>.Failure("Compra no encontrada.");
        return ApiResponse<CompraDto>.Success(_mapper.Map<CompraDto>(compra));
    }

    public async Task<ApiResponse<CompraDto>> CreateAsync(CreateCompraDto dto)
    {
        if (!dto.Detalles.Any())
            return ApiResponse<CompraDto>.Failure("La compra debe tener al menos un producto.");

        foreach (var det in dto.Detalles)
            if (det.Cantidad <= 0 || det.PrecioUnitario <= 0)
                return ApiResponse<CompraDto>.Failure("Cantidad y precio deben ser mayores a cero.");

        var proveedor = await _context.Proveedores.FindAsync(dto.ProveedorId);
        if (proveedor is null || !proveedor.Activo)
            return ApiResponse<CompraDto>.Failure("Proveedor no encontrado.");

        var productosIds = dto.Detalles.Select(d => d.ProductoId).Distinct().ToList();
        var productos = await _context.Productos
            .Where(p => productosIds.Contains(p.Id) && p.Activo)
            .ToDictionaryAsync(p => p.Id);

        foreach (var det in dto.Detalles)
            if (!productos.ContainsKey(det.ProductoId))
                return ApiResponse<CompraDto>.Failure($"Producto con ID {det.ProductoId} no encontrado.");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var numero = $"C-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(1000, 9999)}";
            var compra = new Compra
            {
                NumeroCompra    = numero,
                Fecha           = DateTime.UtcNow,
                ProveedorId     = dto.ProveedorId,
                EstadoCompraId  = EstadoConfirmada,
                Observaciones   = dto.Observaciones
            };

            decimal total = 0;
            var movimientos = new List<MovimientoStock>();

            foreach (var det in dto.Detalles)
            {
                var producto = productos[det.ProductoId];
                var linea = new DetalleCompra
                {
                    ProductoId     = det.ProductoId,
                    Cantidad       = det.Cantidad,
                    PrecioUnitario = det.PrecioUnitario,
                    Subtotal       = det.PrecioUnitario * det.Cantidad
                };
                compra.Detalles.Add(linea);
                total += linea.Subtotal;

                var stockAnterior = producto.StockActual;
                producto.StockActual += det.Cantidad;
                producto.PrecioCompra = det.PrecioUnitario;
                _context.Productos.Update(producto);

                movimientos.Add(new MovimientoStock
                {
                    ProductoId     = det.ProductoId,
                    Cantidad       = det.Cantidad,
                    TipoMovimiento = "COMPRA",
                    Observacion    = $"Compra {numero}",
                    StockAnterior  = stockAnterior,
                    StockPosterior = producto.StockActual
                });
            }

            compra.Total = total;
            await _context.Compras.AddAsync(compra);
            await _context.MovimientosStock.AddRangeAsync(movimientos);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            var result = await GetByIdAsync(compra.Id);
            return ApiResponse<CompraDto>.Success(result.Data!, "Compra registrada correctamente.");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<ApiResponse<bool>> AnularAsync(int id)
    {
        var compra = await _context.Compras
            .Include(c => c.Detalles).ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(c => c.Id == id && c.Activo);

        if (compra is null)
            return ApiResponse<bool>.Failure("Compra no encontrada.");
        if (compra.EstadoCompraId == EstadoAnulada)
            return ApiResponse<bool>.Failure("La compra ya está anulada.");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var movimientos = new List<MovimientoStock>();
            foreach (var det in compra.Detalles)
            {
                var producto = det.Producto;
                var stockAnterior = producto.StockActual;
                producto.StockActual = producto.StockActual >= det.Cantidad
                    ? producto.StockActual - det.Cantidad
                    : 0;
                _context.Productos.Update(producto);

                movimientos.Add(new MovimientoStock
                {
                    ProductoId     = det.ProductoId,
                    Cantidad       = det.Cantidad,
                    TipoMovimiento = "ANULACION_COMPRA",
                    Observacion    = $"Anulación de {compra.NumeroCompra}",
                    StockAnterior  = stockAnterior,
                    StockPosterior = producto.StockActual
                });
            }

            compra.EstadoCompraId = EstadoAnulada;
            await _context.MovimientosStock.AddRangeAsync(movimientos);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return ApiResponse<bool>.Success(true, "Compra anulada y stock revertido.");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<ApiResponse<IEnumerable<CompraDto>>> GetByProveedorAsync(int proveedorId)
    {
        var compras = await _context.Compras
            .Include(c => c.Proveedor)
            .Include(c => c.EstadoCompra)
            .Where(c => c.ProveedorId == proveedorId && c.Activo)
            .OrderByDescending(c => c.Fecha)
            .ToListAsync();
        return ApiResponse<IEnumerable<CompraDto>>.Success(_mapper.Map<IEnumerable<CompraDto>>(compras));
    }
}
