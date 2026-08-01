using Microsoft.EntityFrameworkCore;
using CoreSysHM.Domain.Entities.Compras;
using CoreSysHM.Domain.Interfaces.Repositories;
using CoreSysHM.Infrastructure.Data;

namespace CoreSysHM.Infrastructure.Repositories;

public class CompraRepository : GenericRepository<Compra>, ICompraRepository
{
    public CompraRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Compra>> GetByProveedorAsync(int proveedorId) =>
        await _dbSet.Include(c => c.Proveedor)
                    .Include(c => c.EstadoCompra)
                    .Include(c => c.Detalles).ThenInclude(d => d.Producto)
                    .Where(c => c.ProveedorId == proveedorId && c.Activo)
                    .OrderByDescending(c => c.Fecha)
                    .ToListAsync();

    public async Task<Compra?> GetWithDetallesAsync(int id) =>
        await _dbSet.Include(c => c.Proveedor)
                    .Include(c => c.EstadoCompra)
                    .Include(c => c.RegistradoPor)
                    .Include(c => c.Detalles).ThenInclude(d => d.Producto)
                    .FirstOrDefaultAsync(c => c.Id == id && c.Activo);
}
