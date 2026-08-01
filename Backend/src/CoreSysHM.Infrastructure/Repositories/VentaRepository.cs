using Microsoft.EntityFrameworkCore;
using CoreSysHM.Domain.Entities.Ventas;
using CoreSysHM.Domain.Interfaces.Repositories;
using CoreSysHM.Infrastructure.Data;

namespace CoreSysHM.Infrastructure.Repositories;

public class VentaRepository : GenericRepository<Venta>, IVentaRepository
{
    public VentaRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Venta?> GetByNumeroAsync(string numero) =>
        await _dbSet.Include(v => v.Cliente)
                    .Include(v => v.Detalles).ThenInclude(d => d.Producto)
                    .FirstOrDefaultAsync(v => v.NumeroVenta == numero);

    public async Task<IEnumerable<Venta>> GetByClienteAsync(int clienteId) =>
        await _dbSet.Include(v => v.Cliente)
                    .Include(v => v.Detalles)
                    .Where(v => v.ClienteId == clienteId && v.Activo)
                    .ToListAsync();

    public async Task<IEnumerable<Venta>> GetByFechaAsync(DateTime desde, DateTime hasta) =>
        await _dbSet.Include(v => v.Cliente)
                    .Where(v => v.Fecha >= desde && v.Fecha <= hasta && v.Activo)
                    .ToListAsync();
}
