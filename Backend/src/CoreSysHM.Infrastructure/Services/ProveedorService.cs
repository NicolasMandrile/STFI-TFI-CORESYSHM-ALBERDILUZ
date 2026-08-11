using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using CoreSysHM.Application.Common.Wrappers;
using CoreSysHM.Application.DTOs.Common;
using CoreSysHM.Application.DTOs.Stock;
using CoreSysHM.Application.Interfaces.Services;
using CoreSysHM.Domain.Entities.Stock;
using CoreSysHM.Domain.Exceptions;
using CoreSysHM.Domain.Interfaces;
using CoreSysHM.Infrastructure.Data;

namespace CoreSysHM.Infrastructure.Services;

public class ProveedorService : IProveedorService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ApplicationDbContext _context;
    private readonly IHistorialCambioService _historial;

    public ProveedorService(IUnitOfWork uow, IMapper mapper, ApplicationDbContext context, IHistorialCambioService historial)
    {
        _uow = uow;
        _mapper = mapper;
        _context = context;
        _historial = historial;
    }

    private IQueryable<Proveedor> ConIncludes(IQueryable<Proveedor> query) => query.Include(p => p.CondicionFiscal);

    public async Task<ApiResponse<IEnumerable<ProveedorDto>>> GetAllAsync()
    {
        var proveedores = await ConIncludes(_context.Proveedores.Where(p => p.Activo))
            .OrderBy(p => p.RazonSocial)
            .ToListAsync();
        return ApiResponse<IEnumerable<ProveedorDto>>.Success(_mapper.Map<IEnumerable<ProveedorDto>>(proveedores));
    }

    public async Task<ApiResponse<ProveedorDto>> GetByIdAsync(int id)
    {
        var proveedor = await ConIncludes(_context.Proveedores).FirstOrDefaultAsync(p => p.Id == id && p.Activo);
        if (proveedor is null)
            return ApiResponse<ProveedorDto>.Failure("Proveedor no encontrado.");
        return ApiResponse<ProveedorDto>.Success(_mapper.Map<ProveedorDto>(proveedor));
    }

    public async Task<ApiResponse<ProveedorDto>> CreateAsync(CreateProveedorDto dto, int? usuarioId = null)
    {
        await ValidarUnicidadFiscalAsync(dto.Cuit, excluirId: null);

        var proveedor = _mapper.Map<Proveedor>(dto);
        await _uow.Proveedores.AddAsync(proveedor);

        try
        {
            await _uow.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (EsViolacionDeUnicidad(ex))
        {
            throw new DuplicadoException($"Ya existe un proveedor activo con el CUIT '{dto.Cuit}'.");
        }

        await _historial.RegistrarAsync("Proveedor", proveedor.Id, "Alta", usuarioId, $"Proveedor {proveedor.RazonSocial} creado.");

        var creado = await GetByIdAsync(proveedor.Id);
        return ApiResponse<ProveedorDto>.Success(creado.Data!, "Proveedor creado correctamente.");
    }

    public async Task<ApiResponse<ProveedorDto>> UpdateAsync(int id, CreateProveedorDto dto, int? usuarioId = null)
    {
        var proveedor = await _uow.Proveedores.GetByIdAsync(id);
        if (proveedor is null || !proveedor.Activo)
            return ApiResponse<ProveedorDto>.Failure("Proveedor no encontrado.");

        await ValidarUnicidadFiscalAsync(dto.Cuit, excluirId: id);

        _mapper.Map(dto, proveedor);
        _uow.Proveedores.Update(proveedor);

        try
        {
            await _uow.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (EsViolacionDeUnicidad(ex))
        {
            throw new DuplicadoException($"Ya existe un proveedor activo con el CUIT '{dto.Cuit}'.");
        }

        await _historial.RegistrarAsync("Proveedor", proveedor.Id, "Modificacion", usuarioId, $"Datos de {proveedor.RazonSocial} actualizados.");

        var actualizado = await GetByIdAsync(proveedor.Id);
        return ApiResponse<ProveedorDto>.Success(actualizado.Data!, "Proveedor actualizado correctamente.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id, int? usuarioId = null)
    {
        var proveedor = await _uow.Proveedores.GetByIdAsync(id);
        if (proveedor is null || !proveedor.Activo)
            return ApiResponse<bool>.Failure("Proveedor no encontrado.");

        _uow.Proveedores.Delete(proveedor);
        await _uow.SaveChangesAsync();

        await _historial.RegistrarAsync("Proveedor", proveedor.Id, "BajaLogica", usuarioId, $"Proveedor {proveedor.RazonSocial} dado de baja.");

        return ApiResponse<bool>.Success(true, "Proveedor eliminado correctamente.");
    }

    public async Task<ApiResponse<IEnumerable<HistorialCambioDto>>> GetHistorialAsync(int id)
    {
        var historial = await _historial.GetHistorialAsync("Proveedor", id);
        return ApiResponse<IEnumerable<HistorialCambioDto>>.Success(historial);
    }

    private async Task ValidarUnicidadFiscalAsync(string cuit, int? excluirId)
    {
        if (string.IsNullOrWhiteSpace(cuit)) return;
        var existe = await _context.Proveedores.AnyAsync(p => p.Activo && p.Cuit == cuit && p.Id != (excluirId ?? 0));
        if (existe)
            throw new DuplicadoException($"Ya existe un proveedor activo con el CUIT '{cuit}'.");
    }

    private static bool EsViolacionDeUnicidad(DbUpdateException ex) =>
        ex.InnerException is SqlException sqlEx && (sqlEx.Number == 2601 || sqlEx.Number == 2627);
}
