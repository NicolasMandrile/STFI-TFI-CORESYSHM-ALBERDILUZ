using AutoMapper;
using CoreSysHM.Application.Common.Wrappers;
using CoreSysHM.Application.DTOs.Stock;
using CoreSysHM.Application.Interfaces.Services;
using CoreSysHM.Domain.Entities.Stock;
using CoreSysHM.Domain.Interfaces;

namespace CoreSysHM.Infrastructure.Services;

public class ProveedorService : IProveedorService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public ProveedorService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<ApiResponse<IEnumerable<ProveedorDto>>> GetAllAsync()
    {
        var proveedores = await _uow.Proveedores.GetAllAsync();
        return ApiResponse<IEnumerable<ProveedorDto>>.Success(_mapper.Map<IEnumerable<ProveedorDto>>(proveedores));
    }

    public async Task<ApiResponse<ProveedorDto>> GetByIdAsync(int id)
    {
        var proveedor = await _uow.Proveedores.GetByIdAsync(id);
        if (proveedor is null || !proveedor.Activo)
            return ApiResponse<ProveedorDto>.Failure("Proveedor no encontrado.");
        return ApiResponse<ProveedorDto>.Success(_mapper.Map<ProveedorDto>(proveedor));
    }

    public async Task<ApiResponse<ProveedorDto>> CreateAsync(CreateProveedorDto dto)
    {
        var proveedor = _mapper.Map<Proveedor>(dto);
        await _uow.Proveedores.AddAsync(proveedor);
        await _uow.SaveChangesAsync();
        return ApiResponse<ProveedorDto>.Success(_mapper.Map<ProveedorDto>(proveedor), "Proveedor creado correctamente.");
    }

    public async Task<ApiResponse<ProveedorDto>> UpdateAsync(int id, CreateProveedorDto dto)
    {
        var proveedor = await _uow.Proveedores.GetByIdAsync(id);
        if (proveedor is null || !proveedor.Activo)
            return ApiResponse<ProveedorDto>.Failure("Proveedor no encontrado.");

        _mapper.Map(dto, proveedor);
        _uow.Proveedores.Update(proveedor);
        await _uow.SaveChangesAsync();
        return ApiResponse<ProveedorDto>.Success(_mapper.Map<ProveedorDto>(proveedor), "Proveedor actualizado correctamente.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var proveedor = await _uow.Proveedores.GetByIdAsync(id);
        if (proveedor is null || !proveedor.Activo)
            return ApiResponse<bool>.Failure("Proveedor no encontrado.");

        _uow.Proveedores.Delete(proveedor);
        await _uow.SaveChangesAsync();
        return ApiResponse<bool>.Success(true, "Proveedor eliminado correctamente.");
    }
}
