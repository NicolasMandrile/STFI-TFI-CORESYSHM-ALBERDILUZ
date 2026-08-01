using Microsoft.EntityFrameworkCore;
using CoreSysHM.Domain.Entities.Stock;
using CoreSysHM.Domain.Interfaces.Repositories;
using CoreSysHM.Infrastructure.Data;

namespace CoreSysHM.Infrastructure.Repositories;

public class ProveedorRepository : GenericRepository<Proveedor>, IProveedorRepository
{
    public ProveedorRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Proveedor?> GetByCuitAsync(string cuit) =>
        await _dbSet.FirstOrDefaultAsync(p => p.Cuit == cuit && p.Activo);
}
