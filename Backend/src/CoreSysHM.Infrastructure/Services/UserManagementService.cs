using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CoreSysHM.Application.Common.Wrappers;
using CoreSysHM.Application.DTOs.Usuarios;
using CoreSysHM.Application.Interfaces.Services;
using CoreSysHM.Domain.Entities.Auth;
using CoreSysHM.Domain.Entities.Ventas;
using CoreSysHM.Domain.Enums;
using CoreSysHM.Domain.Security;
using CoreSysHM.Infrastructure.Data;

namespace CoreSysHM.Infrastructure.Services;

public class UserManagementService : IUserManagementService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IAuditoriaService _auditoriaService;
    private readonly ApplicationDbContext _context;

    public UserManagementService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IAuditoriaService auditoriaService,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _auditoriaService = auditoriaService;
        _context = context;
    }

    public async Task<ApiResponse<IEnumerable<UsuarioDto>>> GetAllAsync(string? rol = null, bool? activo = null)
    {
        var query = _userManager.Users.AsQueryable();
        if (activo.HasValue)
            query = query.Where(u => u.IsActive == activo.Value);

        var usuarios = await query.OrderBy(u => u.Nombre).ToListAsync();

        var resultado = new List<UsuarioDto>();
        foreach (var usuario in usuarios)
        {
            var roles = await _userManager.GetRolesAsync(usuario);
            var rolPrincipal = roles.FirstOrDefault() ?? string.Empty;
            if (rol != null && !string.Equals(rolPrincipal, rol, StringComparison.OrdinalIgnoreCase))
                continue;
            var clienteVinculado = await _context.Clientes.FirstOrDefaultAsync(c => c.UserId == usuario.Id);
            resultado.Add(MapToDto(usuario, rolPrincipal, clienteVinculado));
        }

        return ApiResponse<IEnumerable<UsuarioDto>>.Success(resultado);
    }

    public async Task<ApiResponse<UsuarioDto>> GetByIdAsync(int id)
    {
        var usuario = await _userManager.FindByIdAsync(id.ToString());
        if (usuario is null)
            return ApiResponse<UsuarioDto>.Failure("Usuario no encontrado.");

        var roles = await _userManager.GetRolesAsync(usuario);
        var clienteVinculado = await _context.Clientes.FirstOrDefaultAsync(c => c.UserId == usuario.Id);
        return ApiResponse<UsuarioDto>.Success(MapToDto(usuario, roles.FirstOrDefault() ?? string.Empty, clienteVinculado));
    }

    public async Task<ApiResponse<UsuarioDto>> CreateAsync(CreateUsuarioDto dto)
    {
        if (!await _roleManager.RoleExistsAsync(dto.Rol))
            return ApiResponse<UsuarioDto>.Failure("Rol inválido.");

        if (await _userManager.FindByEmailAsync(dto.Email) is not null)
            return ApiResponse<UsuarioDto>.Failure("El email ya está registrado.");

        Cliente? clienteAVincular = null;
        if (dto.Rol == RoleNames.Cliente && dto.ClienteId.HasValue)
        {
            clienteAVincular = await _context.Clientes.FindAsync(dto.ClienteId.Value);
            if (clienteAVincular is null || !clienteAVincular.Activo)
                return ApiResponse<UsuarioDto>.Failure("El cliente seleccionado no existe.");
            if (clienteAVincular.UserId.HasValue)
                return ApiResponse<UsuarioDto>.Failure("Ese cliente ya está vinculado a otro usuario.");
        }

        var usuario = new ApplicationUser
        {
            UserName = dto.NombreUsuario,
            Email = dto.Email,
            EmailConfirmed = true,
            Nombre = dto.Nombre,
            Apellido = dto.Apellido,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var resultado = await _userManager.CreateAsync(usuario, dto.Password);
        if (!resultado.Succeeded)
            return ApiResponse<UsuarioDto>.Failure("No se pudo crear el usuario.", resultado.Errors.Select(e => e.Description));

        await _userManager.AddToRoleAsync(usuario, dto.Rol);

        if (clienteAVincular is not null)
        {
            clienteAVincular.UserId = usuario.Id;
            await _context.SaveChangesAsync();
        }

        return ApiResponse<UsuarioDto>.Success(MapToDto(usuario, dto.Rol, clienteAVincular), "Usuario creado correctamente.");
    }

    public async Task<ApiResponse<UsuarioDto>> UpdateAsync(int id, UpdateUsuarioDto dto)
    {
        var usuario = await _userManager.FindByIdAsync(id.ToString());
        if (usuario is null)
            return ApiResponse<UsuarioDto>.Failure("Usuario no encontrado.");

        if (!await _roleManager.RoleExistsAsync(dto.Rol))
            return ApiResponse<UsuarioDto>.Failure("Rol inválido.");

        var rolesActuales = await _userManager.GetRolesAsync(usuario);
        if (!rolesActuales.Contains(dto.Rol) && rolesActuales.Contains(RoleNames.Administrador))
        {
            var otrosAdminsActivos = await ContarAdministradoresActivosAsync(excluirUsuarioId: usuario.Id);
            if (otrosAdminsActivos == 0)
                return ApiResponse<UsuarioDto>.Failure("No se puede reasignar el rol: es el único Administrador activo.");
        }

        var clienteActual = await _context.Clientes.FirstOrDefaultAsync(c => c.UserId == usuario.Id);
        Cliente? clienteNuevo = clienteActual;

        if (dto.Rol == RoleNames.Cliente && dto.ClienteId.HasValue)
        {
            if (clienteActual is null || clienteActual.Id != dto.ClienteId.Value)
            {
                clienteNuevo = await _context.Clientes.FindAsync(dto.ClienteId.Value);
                if (clienteNuevo is null || !clienteNuevo.Activo)
                    return ApiResponse<UsuarioDto>.Failure("El cliente seleccionado no existe.");
                if (clienteNuevo.UserId.HasValue && clienteNuevo.UserId != usuario.Id)
                    return ApiResponse<UsuarioDto>.Failure("Ese cliente ya está vinculado a otro usuario.");
            }
        }
        else
        {
            clienteNuevo = null; // el rol dejó de ser Cliente, o no se especificó vínculo: se desvincula
        }

        if (clienteActual is not null && clienteActual != clienteNuevo)
            clienteActual.UserId = null;
        if (clienteNuevo is not null)
            clienteNuevo.UserId = usuario.Id;

        usuario.Nombre = dto.Nombre;
        usuario.Apellido = dto.Apellido;
        await _userManager.UpdateAsync(usuario);
        await _context.SaveChangesAsync();

        if (!rolesActuales.Contains(dto.Rol))
        {
            if (rolesActuales.Any())
                await _userManager.RemoveFromRolesAsync(usuario, rolesActuales);
            await _userManager.AddToRoleAsync(usuario, dto.Rol);
        }

        return ApiResponse<UsuarioDto>.Success(MapToDto(usuario, dto.Rol, clienteNuevo), "Usuario actualizado correctamente.");
    }

    public async Task<ApiResponse<bool>> ToggleActivoAsync(int id, bool activo)
    {
        var usuario = await _userManager.FindByIdAsync(id.ToString());
        if (usuario is null)
            return ApiResponse<bool>.Failure("Usuario no encontrado.");

        if (!activo)
        {
            var roles = await _userManager.GetRolesAsync(usuario);
            if (roles.Contains(RoleNames.Administrador))
            {
                var otrosAdminsActivos = await ContarAdministradoresActivosAsync(excluirUsuarioId: usuario.Id);
                if (otrosAdminsActivos == 0)
                    return ApiResponse<bool>.Failure("No se puede desactivar: es el único Administrador activo.");
            }
        }

        usuario.IsActive = activo;
        await _userManager.UpdateAsync(usuario);
        return ApiResponse<bool>.Success(true, activo ? "Usuario activado." : "Usuario desactivado.");
    }

    public async Task<ApiResponse<bool>> ResetPasswordAsync(int id, string nuevaPassword)
    {
        var usuario = await _userManager.FindByIdAsync(id.ToString());
        if (usuario is null)
            return ApiResponse<bool>.Failure("Usuario no encontrado.");

        var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
        var resultado = await _userManager.ResetPasswordAsync(usuario, token, nuevaPassword);
        if (!resultado.Succeeded)
            return ApiResponse<bool>.Failure("No se pudo restablecer la contraseña.", resultado.Errors.Select(e => e.Description));

        var rol = (await _userManager.GetRolesAsync(usuario)).FirstOrDefault();
        await _auditoriaService.RegistrarAsync(usuario.Id, rol, TipoAccionAuditoria.ResetPassword, null, null, "Reset por Administrador");
        return ApiResponse<bool>.Success(true, "Contraseña restablecida correctamente.");
    }

    public async Task<ApiResponse<bool>> CambiarPasswordPropioAsync(int usuarioId, string passwordActual, string passwordNueva)
    {
        var usuario = await _userManager.FindByIdAsync(usuarioId.ToString());
        if (usuario is null)
            return ApiResponse<bool>.Failure("Usuario no encontrado.");

        var resultado = await _userManager.ChangePasswordAsync(usuario, passwordActual, passwordNueva);
        if (!resultado.Succeeded)
            return ApiResponse<bool>.Failure("No se pudo cambiar la contraseña.", resultado.Errors.Select(e => e.Description));

        var rol = (await _userManager.GetRolesAsync(usuario)).FirstOrDefault();
        await _auditoriaService.RegistrarAsync(usuario.Id, rol, TipoAccionAuditoria.ResetPassword, null, null, "Cambio de password propio");
        return ApiResponse<bool>.Success(true, "Contraseña actualizada correctamente.");
    }

    private async Task<int> ContarAdministradoresActivosAsync(int excluirUsuarioId)
    {
        var admins = await _userManager.GetUsersInRoleAsync(RoleNames.Administrador);
        return admins.Count(a => a.Id != excluirUsuarioId && a.IsActive);
    }

    private static UsuarioDto MapToDto(ApplicationUser usuario, string rol, Cliente? clienteVinculado = null) => new()
    {
        Id = usuario.Id,
        NombreUsuario = usuario.UserName ?? string.Empty,
        Email = usuario.Email ?? string.Empty,
        Nombre = usuario.Nombre,
        Apellido = usuario.Apellido,
        Rol = rol,
        IsActive = usuario.IsActive,
        UltimoAcceso = usuario.UltimoAcceso,
        CreatedAt = usuario.CreatedAt,
        ClienteId = clienteVinculado?.Id,
        ClienteNombre = clienteVinculado is null ? null : $"{clienteVinculado.Nombre} {clienteVinculado.Apellido}"
    };
}
