using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using CoreSysHM.Application.Common.Wrappers;
using CoreSysHM.Application.DTOs.Auth;
using CoreSysHM.Application.Interfaces.Services;
using CoreSysHM.Domain.Entities.Auth;
using CoreSysHM.Domain.Enums;

namespace CoreSysHM.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IAuditoriaService _auditoriaService;
    private readonly IConfiguration _configuration;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<ApplicationRole> roleManager,
        IAuditoriaService auditoriaService,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _auditoriaService = auditoriaService;
        _configuration = configuration;
    }

    public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto dto, string? ip, string? userAgent)
    {
        var usuario = await _userManager.FindByEmailAsync(dto.Email);

        if (usuario is null || !usuario.IsActive)
        {
            await _auditoriaService.RegistrarAsync(usuario?.Id, null, TipoAccionAuditoria.LoginFallido, ip, userAgent,
                usuario is null ? "Email no registrado" : "Usuario inactivo");
            return ApiResponse<LoginResponseDto>.Failure("Credenciales inválidas.");
        }

        var resultado = await _signInManager.CheckPasswordSignInAsync(usuario, dto.Password, lockoutOnFailure: true);
        if (!resultado.Succeeded)
        {
            var motivo = resultado.IsLockedOut ? "Cuenta bloqueada temporalmente por intentos fallidos" : "Credenciales inválidas";
            await _auditoriaService.RegistrarAsync(usuario.Id, null, TipoAccionAuditoria.LoginFallido, ip, userAgent, motivo);
            return ApiResponse<LoginResponseDto>.Failure("Credenciales inválidas.");
        }

        var roles = await _userManager.GetRolesAsync(usuario);
        var rolPrincipal = roles.FirstOrDefault() ?? string.Empty;
        var permisos = await GetPermisosEfectivosAsync(roles);

        usuario.UltimoAcceso = DateTime.UtcNow;
        await _userManager.UpdateAsync(usuario);

        var token = GenerarToken(usuario, roles, permisos);
        var expiracionHoras = int.TryParse(_configuration["Jwt:ExpirationHours"], out var h) ? h : 8;

        await _auditoriaService.RegistrarAsync(usuario.Id, rolPrincipal, TipoAccionAuditoria.Login, ip, userAgent);

        return ApiResponse<LoginResponseDto>.Success(new LoginResponseDto
        {
            Token = token,
            Expiracion = DateTime.UtcNow.AddHours(expiracionHoras),
            NombreUsuario = usuario.UserName ?? usuario.Email ?? string.Empty,
            Rol = rolPrincipal,
            Permisos = permisos
        });
    }

    /// <summary>
    /// Los permisos efectivos SIEMPRE se leen de ApplicationRole.Permissions (BD, editable por el
    /// Administrador vía RoleManagementService) -- RolePermissions (clase estática) es solo para
    /// el seed inicial y tests, nunca para esta consulta en runtime.
    /// </summary>
    private async Task<List<string>> GetPermisosEfectivosAsync(IEnumerable<string> roles)
    {
        var permisos = new HashSet<string>();
        foreach (var nombreRol in roles)
        {
            var rol = await _roleManager.FindByNameAsync(nombreRol);
            if (rol is null) continue;

            if (rol.Name == Domain.Security.RoleNames.Administrador)
                return Domain.Security.Permissions.All().ToList();

            foreach (var permiso in rol.Permissions)
                permisos.Add(permiso);
        }
        return permisos.ToList();
    }

    private string GenerarToken(ApplicationUser usuario, IEnumerable<string> roles, IEnumerable<string> permisos)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expHoras = int.TryParse(_configuration["Jwt:ExpirationHours"], out var h) ? h : 8;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Email ?? string.Empty),
            new(ClaimTypes.Name, usuario.UserName ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
        claims.AddRange(permisos.Select(p => new Claim("permission", p)));

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(expHoras),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
