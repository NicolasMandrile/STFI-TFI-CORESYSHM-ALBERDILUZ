using CoreSysHM.Domain.Entities.Compras;

namespace CoreSysHM.Domain.Interfaces.Repositories;

public interface ICompraRepository : IGenericRepository<Compra>
{
    Task<IEnumerable<Compra>> GetByProveedorAsync(int proveedorId);
    Task<Compra?> GetWithDetallesAsync(int id);
}
