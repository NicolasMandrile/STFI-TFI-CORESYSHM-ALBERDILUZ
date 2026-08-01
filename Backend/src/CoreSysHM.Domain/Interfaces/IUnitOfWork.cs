using CoreSysHM.Domain.Entities.Common;
using CoreSysHM.Domain.Interfaces.Repositories;

namespace CoreSysHM.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IProductoRepository Productos { get; }
    IVentaRepository Ventas { get; }
    IFacturaRepository Facturas { get; }
    IProveedorRepository Proveedores { get; }
    ICompraRepository Compras { get; }
    IGenericRepository<T> Repository<T>() where T : BaseEntity;
    Task<int> SaveChangesAsync();
}
