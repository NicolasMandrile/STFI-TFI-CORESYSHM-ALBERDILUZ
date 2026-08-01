using AutoMapper;
using CoreSysHM.Application.Common.Wrappers;
using CoreSysHM.Application.DTOs.Stock;
using CoreSysHM.Application.Interfaces.Services;
using CoreSysHM.Domain.Entities.Stock;
using CoreSysHM.Domain.Interfaces;

namespace CoreSysHM.Infrastructure.Services;

public class CategoriaService : ICategoriaService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public CategoriaService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<ApiResponse<IEnumerable<CategoriaDto>>> GetAllAsync()
    {
        var cats = await _uow.Repository<Categoria>().GetAllAsync();
        return ApiResponse<IEnumerable<CategoriaDto>>.Success(_mapper.Map<IEnumerable<CategoriaDto>>(cats));
    }

    public async Task<ApiResponse<CategoriaDto>> GetByIdAsync(int id)
    {
        var cat = await _uow.Repository<Categoria>().GetByIdAsync(id);
        if (cat is null || !cat.Activo)
            return ApiResponse<CategoriaDto>.Failure("Categoría no encontrada.");
        return ApiResponse<CategoriaDto>.Success(_mapper.Map<CategoriaDto>(cat));
    }

    public async Task<ApiResponse<CategoriaDto>> CreateAsync(CreateCategoriaDto dto)
    {
        var cat = _mapper.Map<Categoria>(dto);
        await _uow.Repository<Categoria>().AddAsync(cat);
        await _uow.SaveChangesAsync();
        return ApiResponse<CategoriaDto>.Success(_mapper.Map<CategoriaDto>(cat), "Categoría creada correctamente.");
    }

    public async Task<ApiResponse<CategoriaDto>> UpdateAsync(int id, CreateCategoriaDto dto)
    {
        var cat = await _uow.Repository<Categoria>().GetByIdAsync(id);
        if (cat is null || !cat.Activo)
            return ApiResponse<CategoriaDto>.Failure("Categoría no encontrada.");

        _mapper.Map(dto, cat);
        _uow.Repository<Categoria>().Update(cat);
        await _uow.SaveChangesAsync();
        return ApiResponse<CategoriaDto>.Success(_mapper.Map<CategoriaDto>(cat), "Categoría actualizada correctamente.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var cat = await _uow.Repository<Categoria>().GetByIdAsync(id);
        if (cat is null || !cat.Activo)
            return ApiResponse<bool>.Failure("Categoría no encontrada.");

        _uow.Repository<Categoria>().Delete(cat);
        await _uow.SaveChangesAsync();
        return ApiResponse<bool>.Success(true, "Categoría eliminada correctamente.");
    }
}
