using Microsoft.EntityFrameworkCore;
using CoreSysHM.Domain.Entities.Facturacion;
using CoreSysHM.Domain.Enums;
using CoreSysHM.Domain.Interfaces.Repositories;
using CoreSysHM.Infrastructure.Data;

namespace CoreSysHM.Infrastructure.Repositories;

public class FacturaRepository : GenericRepository<Factura>, IFacturaRepository
{
    public FacturaRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Factura?> GetByNumeroAsync(string numero) =>
        await _dbSet.Include(f => f.Cliente)
                    .Include(f => f.Venta)
                    .FirstOrDefaultAsync(f => f.NumeroFactura == numero);

    public async Task<IEnumerable<Factura>> GetByClienteAsync(int clienteId) =>
        await _dbSet.Include(f => f.Cliente)
                    .Include(f => f.Venta)
                    .Where(f => f.ClienteId == clienteId && f.Activo)
                    .ToListAsync();

    public async Task<IEnumerable<Factura>> GetVencidasAsync() =>
        await _dbSet.Include(f => f.Cliente)
                    .Where(f => f.FechaVencimiento < DateTime.UtcNow
                             && f.Estado == EstadoFactura.Emitida
                             && f.Activo)
                    .ToListAsync();
}
