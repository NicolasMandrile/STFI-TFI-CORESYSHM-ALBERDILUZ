using CoreSysHM.Domain.Entities.Ventas;

namespace CoreSysHM.Domain.Interfaces.Repositories;

public interface IVentaRepository : IGenericRepository<Venta>
{
    Task<Venta?> GetByNumeroAsync(string numero);
    Task<IEnumerable<Venta>> GetByClienteAsync(int clienteId);
    Task<IEnumerable<Venta>> GetByFechaAsync(DateTime desde, DateTime hasta);
}
