using CoreSysHM.Domain.Entities.Facturacion;

namespace CoreSysHM.Domain.Interfaces.Repositories;

public interface IFacturaRepository : IGenericRepository<Factura>
{
    Task<Factura?> GetByNumeroAsync(string numero);
    Task<IEnumerable<Factura>> GetByClienteAsync(int clienteId);
    Task<IEnumerable<Factura>> GetVencidasAsync();
}
