using Microsoft.EntityFrameworkCore;
using CoreSysHM.Domain.Entities.Stock;
using CoreSysHM.Domain.Interfaces.Repositories;
using CoreSysHM.Infrastructure.Data;

namespace CoreSysHM.Infrastructure.Repositories;

public class ProductoRepository : GenericRepository<Producto>, IProductoRepository
{
    public ProductoRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Producto?> GetByCodigoAsync(string codigo) =>
        await _dbSet.Include(p => p.Categoria)
                    .FirstOrDefaultAsync(p => p.Codigo == codigo && p.Activo);

    public async Task<IEnumerable<Producto>> GetByCategoriaAsync(int categoriaId) =>
        await _dbSet.Include(p => p.Categoria)
                    .Where(p => p.CategoriaId == categoriaId && p.Activo)
                    .ToListAsync();

    public async Task<IEnumerable<Producto>> GetStockBajoAsync() =>
        await _dbSet.Include(p => p.Categoria)
                    .Where(p => p.StockActual <= p.StockMinimo && p.Activo)
                    .ToListAsync();
}
