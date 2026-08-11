using Microsoft.EntityFrameworkCore;
using CoreSysHM.Application.DTOs.Common;
using CoreSysHM.Application.Interfaces.Services;
using CoreSysHM.Domain.Entities.Common;
using CoreSysHM.Infrastructure.Data;

namespace CoreSysHM.Infrastructure.Services;

public class HistorialCambioService : IHistorialCambioService
{
    private readonly ApplicationDbContext _context;

    public HistorialCambioService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task RegistrarAsync(string entidad, int entidadId, string accion, int? usuarioId, string? detalle = null)
    {
        _context.HistorialCambios.Add(new HistorialCambio
        {
            Entidad = entidad,
            EntidadId = entidadId,
            Accion = accion,
            UsuarioId = usuarioId,
            Detalle = detalle
        });
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<HistorialCambioDto>> GetHistorialAsync(string entidad, int entidadId)
    {
        return await _context.HistorialCambios
            .Include(h => h.Usuario)
            .Where(h => h.Entidad == entidad && h.EntidadId == entidadId)
            .OrderByDescending(h => h.Fecha)
            .Select(h => new HistorialCambioDto
            {
                Fecha = h.Fecha,
                Accion = h.Accion,
                UsuarioNombre = h.Usuario != null ? h.Usuario.Nombre + " " + h.Usuario.Apellido : null,
                Detalle = h.Detalle
            })
            .ToListAsync();
    }
}
