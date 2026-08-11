using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using CoreSysHM.Application.Common.Wrappers;
using CoreSysHM.Application.DTOs.Common;
using CoreSysHM.Application.DTOs.Ventas;
using CoreSysHM.Application.Interfaces.Services;
using CoreSysHM.Domain.Entities.Ventas;
using CoreSysHM.Domain.Exceptions;
using CoreSysHM.Infrastructure.Data;

namespace CoreSysHM.Infrastructure.Services;

public class ClienteService : IClienteService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IHistorialCambioService _historial;

    public ClienteService(ApplicationDbContext context, IMapper mapper, IHistorialCambioService historial)
    {
        _context = context;
        _mapper = mapper;
        _historial = historial;
    }

    private IQueryable<Cliente> ConIncludes(IQueryable<Cliente> query) => query.Include(c => c.CondicionFiscal);

    public async Task<ApiResponse<IEnumerable<ClienteDto>>> GetAllAsync()
    {
        var clientes = await ConIncludes(_context.Clientes.Where(c => c.Activo))
            .OrderBy(c => c.Apellido).ThenBy(c => c.Nombre)
            .ToListAsync();
        return ApiResponse<IEnumerable<ClienteDto>>.Success(_mapper.Map<IEnumerable<ClienteDto>>(clientes));
    }

    public async Task<ApiResponse<ClienteDto>> GetByIdAsync(int id)
    {
        var cliente = await ConIncludes(_context.Clientes).FirstOrDefaultAsync(c => c.Id == id && c.Activo);
        if (cliente is null)
            return ApiResponse<ClienteDto>.Failure("Cliente no encontrado.");
        return ApiResponse<ClienteDto>.Success(_mapper.Map<ClienteDto>(cliente));
    }

    public async Task<ApiResponse<ClienteDto>> CreateAsync(CreateClienteDto dto, int? usuarioId)
    {
        await ValidarUnicidadFiscalAsync(dto.Dni, dto.Cuit, excluirId: null);

        var cliente = _mapper.Map<Cliente>(dto);
        await _context.Clientes.AddAsync(cliente);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (EsViolacionDeUnicidad(ex))
        {
            throw new DuplicadoException("Ya existe un cliente activo con ese DNI o CUIT.");
        }

        await _historial.RegistrarAsync("Cliente", cliente.Id, "Alta", usuarioId,
            $"Cliente {cliente.Nombre} {cliente.Apellido} creado.");

        var creado = await GetByIdAsync(cliente.Id);
        return ApiResponse<ClienteDto>.Success(creado.Data!, "Cliente creado correctamente.");
    }

    public async Task<ApiResponse<ClienteDto>> UpdateAsync(int id, CreateClienteDto dto, int? usuarioId)
    {
        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == id && c.Activo);
        if (cliente is null)
            return ApiResponse<ClienteDto>.Failure("Cliente no encontrado.");

        await ValidarUnicidadFiscalAsync(dto.Dni, dto.Cuit, excluirId: id);

        _mapper.Map(dto, cliente);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (EsViolacionDeUnicidad(ex))
        {
            throw new DuplicadoException("Ya existe un cliente activo con ese DNI o CUIT.");
        }

        await _historial.RegistrarAsync("Cliente", cliente.Id, "Modificacion", usuarioId,
            $"Datos de {cliente.Nombre} {cliente.Apellido} actualizados.");

        var actualizado = await GetByIdAsync(cliente.Id);
        return ApiResponse<ClienteDto>.Success(actualizado.Data!, "Cliente actualizado correctamente.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id, int? usuarioId)
    {
        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == id && c.Activo);
        if (cliente is null)
            return ApiResponse<bool>.Failure("Cliente no encontrado.");

        cliente.Activo = false;
        cliente.FechaModificacion = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _historial.RegistrarAsync("Cliente", cliente.Id, "BajaLogica", usuarioId,
            $"Cliente {cliente.Nombre} {cliente.Apellido} dado de baja.");

        return ApiResponse<bool>.Success(true, "Cliente eliminado correctamente.");
    }

    public async Task<ApiResponse<IEnumerable<HistorialCambioDto>>> GetHistorialAsync(int id)
    {
        var historial = await _historial.GetHistorialAsync("Cliente", id);
        return ApiResponse<IEnumerable<HistorialCambioDto>>.Success(historial);
    }

    private async Task ValidarUnicidadFiscalAsync(string? dni, string? cuit, int? excluirId)
    {
        if (!string.IsNullOrWhiteSpace(dni))
        {
            var existeDni = await _context.Clientes.AnyAsync(c => c.Activo && c.Dni == dni && c.Id != (excluirId ?? 0));
            if (existeDni)
                throw new DuplicadoException($"Ya existe un cliente activo con el DNI '{dni}'.");
        }
        if (!string.IsNullOrWhiteSpace(cuit))
        {
            var existeCuit = await _context.Clientes.AnyAsync(c => c.Activo && c.Cuit == cuit && c.Id != (excluirId ?? 0));
            if (existeCuit)
                throw new DuplicadoException($"Ya existe un cliente activo con el CUIT '{cuit}'.");
        }
    }

    private static bool EsViolacionDeUnicidad(DbUpdateException ex) =>
        ex.InnerException is SqlException sqlEx && (sqlEx.Number == 2601 || sqlEx.Number == 2627);
}
