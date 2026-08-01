using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CoreSysHM.Application.Common.Wrappers;
using CoreSysHM.Application.DTOs.Stock;
using CoreSysHM.Application.Interfaces.Services;
using CoreSysHM.Domain.Entities.Stock;
using CoreSysHM.Domain.Interfaces;
using CoreSysHM.Infrastructure.Data;

namespace CoreSysHM.Infrastructure.Services;

public class ProductoService : IProductoService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ApplicationDbContext _context;

    public ProductoService(IUnitOfWork uow, IMapper mapper, ApplicationDbContext context)
    {
        _uow = uow;
        _mapper = mapper;
        _context = context;
    }

    public async Task<ApiResponse<IEnumerable<ProductoDto>>> GetAllAsync()
    {
        var productos = await _context.Productos
            .Include(p => p.Categoria)
            .Include(p => p.Proveedor)
            .Where(p => p.Activo)
            .ToListAsync();
        return ApiResponse<IEnumerable<ProductoDto>>.Success(_mapper.Map<IEnumerable<ProductoDto>>(productos));
    }

    public async Task<ApiResponse<ProductoDto>> GetByIdAsync(int id)
    {
        var producto = await _context.Productos
            .Include(p => p.Categoria)
            .Include(p => p.Proveedor)
            .FirstOrDefaultAsync(p => p.Id == id && p.Activo);

        if (producto is null)
            return ApiResponse<ProductoDto>.Failure("Producto no encontrado.");

        return ApiResponse<ProductoDto>.Success(_mapper.Map<ProductoDto>(producto));
    }

    public async Task<ApiResponse<ProductoDto>> CreateAsync(CreateProductoDto dto)
    {
        var existe = await _context.Productos.AnyAsync(p => p.Codigo == dto.Codigo && p.Activo);
        if (existe)
            return ApiResponse<ProductoDto>.Failure($"Ya existe un producto con código '{dto.Codigo}'.");

        var producto = _mapper.Map<Producto>(dto);
        await _uow.Productos.AddAsync(producto);
        await _uow.SaveChangesAsync();

        await _context.Entry(producto).Reference(p => p.Categoria).LoadAsync();
        if (producto.ProveedorId.HasValue)
            await _context.Entry(producto).Reference(p => p.Proveedor).LoadAsync();

        return ApiResponse<ProductoDto>.Success(_mapper.Map<ProductoDto>(producto), "Producto creado correctamente.");
    }

    public async Task<ApiResponse<ProductoDto>> UpdateAsync(int id, UpdateProductoDto dto)
    {
        var producto = await _context.Productos.Include(p => p.Categoria).Include(p => p.Proveedor)
            .FirstOrDefaultAsync(p => p.Id == id && p.Activo);

        if (producto is null)
            return ApiResponse<ProductoDto>.Failure("Producto no encontrado.");

        _mapper.Map(dto, producto);
        _uow.Productos.Update(producto);
        await _uow.SaveChangesAsync();

        await _context.Entry(producto).Reference(p => p.Categoria).LoadAsync();

        return ApiResponse<ProductoDto>.Success(_mapper.Map<ProductoDto>(producto), "Producto actualizado correctamente.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var producto = await _uow.Productos.GetByIdAsync(id);
        if (producto is null)
            return ApiResponse<bool>.Failure("Producto no encontrado.");

        _uow.Productos.Delete(producto);
        await _uow.SaveChangesAsync();
        return ApiResponse<bool>.Success(true, "Producto eliminado correctamente.");
    }

    public async Task<ApiResponse<IEnumerable<ProductoDto>>> GetStockBajoAsync()
    {
        var productos = await _uow.Productos.GetStockBajoAsync();
        return ApiResponse<IEnumerable<ProductoDto>>.Success(_mapper.Map<IEnumerable<ProductoDto>>(productos));
    }

    public async Task<ApiResponse<bool>> AjustarStockAsync(int id, int cantidad, string tipoMovimiento, string? observacion)
    {
        if (cantidad <= 0)
            return ApiResponse<bool>.Failure("La cantidad debe ser mayor a cero.");

        var producto = await _uow.Productos.GetByIdAsync(id);
        if (producto is null)
            return ApiResponse<bool>.Failure("Producto no encontrado.");

        var stockAnterior = producto.StockActual;

        var tipo = tipoMovimiento.ToUpper();
        if (tipo == "ENTRADA" || tipo == "AJUSTE")
            producto.StockActual += cantidad;
        else if (tipo == "SALIDA" || tipo == "PERDIDA" || tipo == "RECUENTO")
        {
            if (producto.StockActual < cantidad)
                return ApiResponse<bool>.Failure("Stock insuficiente para realizar el movimiento.");
            producto.StockActual -= cantidad;
        }
        else
            return ApiResponse<bool>.Failure($"Tipo de movimiento '{tipoMovimiento}' no reconocido.");

        var movimiento = new MovimientoStock
        {
            ProductoId = id,
            Cantidad = cantidad,
            TipoMovimiento = tipo,
            Observacion = observacion,
            StockAnterior = stockAnterior,
            StockPosterior = producto.StockActual
        };

        _uow.Productos.Update(producto);
        await _context.MovimientosStock.AddAsync(movimiento);
        await _uow.SaveChangesAsync();

        return ApiResponse<bool>.Success(true, "Stock ajustado correctamente.");
    }
}
