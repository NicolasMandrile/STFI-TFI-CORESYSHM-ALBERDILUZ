using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CoreSysHM.Application.Common.Wrappers;
using CoreSysHM.Application.DTOs.Stock;
using CoreSysHM.Application.Interfaces.Services;
using CoreSysHM.Domain.Entities.Stock;
using CoreSysHM.Infrastructure.Data;

namespace CoreSysHM.Infrastructure.Services;

public class MovimientoStockService : IMovimientoStockService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public MovimientoStockService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ApiResponse<IEnumerable<MovimientoStockDto>>> GetAllAsync()
    {
        var movimientos = await _context.MovimientosStock
            .Include(m => m.Producto)
            .OrderByDescending(m => m.FechaCreacion)
            .Take(500)
            .ToListAsync();
        return ApiResponse<IEnumerable<MovimientoStockDto>>.Success(_mapper.Map<IEnumerable<MovimientoStockDto>>(movimientos));
    }

    public async Task<ApiResponse<IEnumerable<MovimientoStockDto>>> GetByProductoAsync(int productoId)
    {
        var movimientos = await _context.MovimientosStock
            .Include(m => m.Producto)
            .Where(m => m.ProductoId == productoId)
            .OrderByDescending(m => m.FechaCreacion)
            .ToListAsync();
        return ApiResponse<IEnumerable<MovimientoStockDto>>.Success(_mapper.Map<IEnumerable<MovimientoStockDto>>(movimientos));
    }

    public async Task<ApiResponse<MovimientoStockDto>> RegistrarMovimientoAsync(CreateMovimientoStockDto dto)
    {
        if (dto.Cantidad <= 0)
            return ApiResponse<MovimientoStockDto>.Failure("La cantidad debe ser mayor a cero.");

        var producto = await _context.Productos.FindAsync(dto.ProductoId);
        if (producto is null || !producto.Activo)
            return ApiResponse<MovimientoStockDto>.Failure("Producto no encontrado.");

        var tipo = dto.TipoMovimiento.ToUpper();
        var stockAnterior = producto.StockActual;

        if (tipo == "ENTRADA" || tipo == "AJUSTE")
            producto.StockActual += dto.Cantidad;
        else if (tipo is "SALIDA" or "PERDIDA" or "RECUENTO")
        {
            if (producto.StockActual < dto.Cantidad)
                return ApiResponse<MovimientoStockDto>.Failure("Stock insuficiente.");
            producto.StockActual -= dto.Cantidad;
        }
        else
            return ApiResponse<MovimientoStockDto>.Failure($"Tipo de movimiento '{dto.TipoMovimiento}' no válido.");

        var movimiento = new MovimientoStock
        {
            ProductoId = dto.ProductoId,
            Cantidad = dto.Cantidad,
            TipoMovimiento = tipo,
            Observacion = dto.Observacion,
            StockAnterior = stockAnterior,
            StockPosterior = producto.StockActual
        };

        _context.Productos.Update(producto);
        await _context.MovimientosStock.AddAsync(movimiento);
        await _context.SaveChangesAsync();

        movimiento.Producto = producto;
        return ApiResponse<MovimientoStockDto>.Success(_mapper.Map<MovimientoStockDto>(movimiento), "Movimiento registrado correctamente.");
    }
}
