using CoreSysHM.Domain.Entities.Stock;

namespace CoreSysHM.Domain.Interfaces.Repositories;

public interface IProveedorRepository : IGenericRepository<Proveedor>
{
    Task<Proveedor?> GetByCuitAsync(string cuit);
}
