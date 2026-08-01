using CoreSysHM.Domain.Entities.Stock;

namespace CoreSysHM.Domain.Interfaces.Repositories;

public interface IProductoRepository : IGenericRepository<Producto>
{
    Task<Producto?> GetByCodigoAsync(string codigo);
    Task<IEnumerable<Producto>> GetByCategoriaAsync(int categoriaId);
    Task<IEnumerable<Producto>> GetStockBajoAsync();
}
