using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CoreSysHM.Application.Common.Wrappers;
using CoreSysHM.Application.DTOs.Roles;
using CoreSysHM.Application.Interfaces.Services;
using CoreSysHM.Domain.Entities.Auth;
using CoreSysHM.Domain.Security;

namespace CoreSysHM.Infrastructure.Services;

public class RoleManagementService : IRoleManagementService
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public RoleManagementService(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task<ApiResponse<IEnumerable<RoleDto>>> GetAllAsync()
    {
        var roles = await _roleManager.Roles.OrderBy(r => r.Name).ToListAsync();
        var resultado = new List<RoleDto>();
        foreach (var role in roles)
        {
            var usuarios = await _userManager.GetUsersInRoleAsync(role.Name!);
            resultado.Add(MapToDto(role, usuarios.Count));
        }
        return ApiResponse<IEnumerable<RoleDto>>.Success(resultado);
    }

    public async Task<ApiResponse<RoleDto>> GetByIdAsync(int id)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());
        if (role is null)
            return ApiResponse<RoleDto>.Failure("Rol no encontrado.");

        var usuarios = await _userManager.GetUsersInRoleAsync(role.Name!);
        return ApiResponse<RoleDto>.Success(MapToDto(role, usuarios.Count));
    }

    public async Task<ApiResponse<RoleDto>> CreateAsync(CreateRoleDto dto)
    {
        if (await _roleManager.FindByNameAsync(dto.Name) is not null)
            return ApiResponse<RoleDto>.Failure("Ya existe un rol con ese nombre.");

        var permisosInvalidos = dto.Permissions.Except(Permissions.All()).ToList();
        if (permisosInvalidos.Any())
            return ApiResponse<RoleDto>.Failure($"Permisos inválidos: {string.Join(", ", permisosInvalidos)}");

        var role = new ApplicationRole
        {
            Name = dto.Name,
            Description = dto.Description,
            IsActive = true,
            IsSystem = false,
            IsSeeded = false,
            CreatedAt = DateTime.UtcNow,
            Permissions = dto.Permissions.Distinct().ToList()
        };

        var resultado = await _roleManager.CreateAsync(role);
        if (!resultado.Succeeded)
            return ApiResponse<RoleDto>.Failure("No se pudo crear el rol.", resultado.Errors.Select(e => e.Description));

        return ApiResponse<RoleDto>.Success(MapToDto(role, 0), "Rol creado correctamente.");
    }

    public async Task<ApiResponse<RoleDto>> UpdateAsync(int id, UpdateRoleDto dto)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());
        if (role is null)
            return ApiResponse<RoleDto>.Failure("Rol no encontrado.");

        if (role.IsSystem)
            return ApiResponse<RoleDto>.Failure("El rol de sistema (Administrador) no es editable: siempre tiene acceso total.");

        if (!string.Equals(role.Name, dto.Name, StringComparison.Ordinal))
        {
            if (role.IsSeeded)
                return ApiResponse<RoleDto>.Failure("No se puede renombrar un rol protegido de fábrica.");

            if (await _roleManager.FindByNameAsync(dto.Name) is not null)
                return ApiResponse<RoleDto>.Failure("Ya existe un rol con ese nombre.");

            role.Name = dto.Name;
        }

        var permisosInvalidos = dto.Permissions.Except(Permissions.All()).ToList();
        if (permisosInvalidos.Any())
            return ApiResponse<RoleDto>.Failure($"Permisos inválidos: {string.Join(", ", permisosInvalidos)}");

        role.Description = dto.Description;
        role.IsActive = dto.IsActive;
        role.Permissions = dto.Permissions.Distinct().ToList();

        var resultado = await _roleManager.UpdateAsync(role);
        if (!resultado.Succeeded)
            return ApiResponse<RoleDto>.Failure("No se pudo actualizar el rol.", resultado.Errors.Select(e => e.Description));

        var usuarios = await _userManager.GetUsersInRoleAsync(role.Name!);
        return ApiResponse<RoleDto>.Success(MapToDto(role, usuarios.Count),
            "Rol actualizado. Los cambios de permisos tienen efecto a partir del próximo login de cada usuario.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());
        if (role is null)
            return ApiResponse<bool>.Failure("Rol no encontrado.");

        if (role.IsSystem)
            return ApiResponse<bool>.Failure("El rol de sistema (Administrador) no se puede eliminar.");

        if (role.IsSeeded)
            return ApiResponse<bool>.Failure("Los roles protegidos (sembrados de fábrica) no se pueden eliminar.");

        var usuarios = await _userManager.GetUsersInRoleAsync(role.Name!);
        if (usuarios.Any())
            return ApiResponse<bool>.Failure("No se puede eliminar un rol con usuarios asignados.");

        var resultado = await _roleManager.DeleteAsync(role);
        if (!resultado.Succeeded)
            return ApiResponse<bool>.Failure("No se pudo eliminar el rol.", resultado.Errors.Select(e => e.Description));

        return ApiResponse<bool>.Success(true, "Rol eliminado correctamente.");
    }

    public Task<ApiResponse<IReadOnlyList<string>>> GetCatalogoPermisosAsync() =>
        Task.FromResult(ApiResponse<IReadOnlyList<string>>.Success(Permissions.All()));

    private static RoleDto MapToDto(ApplicationRole role, int cantidadUsuarios) => new()
    {
        Id = role.Id,
        Name = role.Name ?? string.Empty,
        Description = role.Description,
        IsActive = role.IsActive,
        IsSystem = role.IsSystem,
        IsSeeded = role.IsSeeded,
        Permissions = role.Permissions,
        CantidadUsuarios = cantidadUsuarios,
        CreatedAt = role.CreatedAt
    };
}
