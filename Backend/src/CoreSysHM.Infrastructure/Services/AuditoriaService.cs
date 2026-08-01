using Microsoft.EntityFrameworkCore;
using CoreSysHM.Application.Common.Wrappers;
using CoreSysHM.Application.DTOs.Auditoria;
using CoreSysHM.Application.Interfaces.Services;
using CoreSysHM.Domain.Entities.Auth;
using CoreSysHM.Domain.Enums;
using CoreSysHM.Infrastructure.Data;

namespace CoreSysHM.Infrastructure.Services;

public class AuditoriaService : IAuditoriaService
{
    private readonly ApplicationDbContext _context;

    public AuditoriaService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task RegistrarAsync(int? usuarioId, string? rolSnapshot, TipoAccionAuditoria accion,
        string? ip, string? userAgent, string? detalle = null)
    {
        _context.AuditoriasAcceso.Add(new AuditoriaAcceso
        {
            UsuarioId = usuarioId,
            RolSnapshot = rolSnapshot,
            Accion = accion,
            Ip = ip,
            UserAgent = userAgent,
            Detalle = detalle,
            Timestamp = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
    }

    public async Task<ApiResponse<PagedResponse<AuditoriaAccesoDto>>> BuscarAsync(AuditoriaFiltroDto filtro)
    {
        var query = _context.AuditoriasAcceso.AsNoTracking().Include(a => a.Usuario).AsQueryable();

        if (filtro.UsuarioId.HasValue)
            query = query.Where(a => a.UsuarioId == filtro.UsuarioId);

        if (!string.IsNullOrWhiteSpace(filtro.Accion) && Enum.TryParse<TipoAccionAuditoria>(filtro.Accion, true, out var accion))
            query = query.Where(a => a.Accion == accion);

        if (filtro.FechaDesde.HasValue)
            query = query.Where(a => a.Timestamp >= filtro.FechaDesde.Value);

        if (filtro.FechaHasta.HasValue)
            query = query.Where(a => a.Timestamp <= filtro.FechaHasta.Value);

        var total = await query.CountAsync();

        var pagina = Math.Max(filtro.Pagina, 1);
        var tamanoPagina = Math.Clamp(filtro.TamanoPagina, 1, 200);

        var items = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .Select(a => new AuditoriaAccesoDto
            {
                Id = a.Id,
                UsuarioId = a.UsuarioId,
                UsuarioNombre = a.Usuario != null ? $"{a.Usuario.Nombre} {a.Usuario.Apellido}" : null,
                UsuarioEmail = a.Usuario != null ? a.Usuario.Email : null,
                RolSnapshot = a.RolSnapshot,
                Accion = a.Accion.ToString(),
                Ip = a.Ip,
                UserAgent = a.UserAgent,
                Timestamp = a.Timestamp,
                Detalle = a.Detalle
            })
            .ToListAsync();

        return ApiResponse<PagedResponse<AuditoriaAccesoDto>>.Success(new PagedResponse<AuditoriaAccesoDto>
        {
            Items = items,
            TotalRegistros = total,
            Pagina = pagina,
            TamanoPagina = tamanoPagina
        });
    }
}
