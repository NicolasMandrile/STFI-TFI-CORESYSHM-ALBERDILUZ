using CoreSysHM.Domain.Entities.Common;
using CoreSysHM.Domain.Interfaces;
using CoreSysHM.Domain.Interfaces.Repositories;
using CoreSysHM.Infrastructure.Data;

namespace CoreSysHM.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly Dictionary<Type, object> _repositories = new();

    public IProductoRepository Productos { get; }
    public IVentaRepository Ventas { get; }
    public IFacturaRepository Facturas { get; }
    public IProveedorRepository Proveedores { get; }
    public ICompraRepository Compras { get; }

    public UnitOfWork(ApplicationDbContext context,
        IProductoRepository productos,
        IVentaRepository ventas,
        IFacturaRepository facturas,
        IProveedorRepository proveedores,
        ICompraRepository compras)
    {
        _context = context;
        Productos = productos;
        Ventas = ventas;
        Facturas = facturas;
        Proveedores = proveedores;
        Compras = compras;
    }

    public IGenericRepository<T> Repository<T>() where T : BaseEntity
    {
        var type = typeof(T);
        if (!_repositories.ContainsKey(type))
            _repositories[type] = new GenericRepository<T>(_context);
        return (IGenericRepository<T>)_repositories[type];
    }

    public async Task<int> SaveChangesAsync() =>
        await _context.SaveChangesAsync();

    public void Dispose() => _context.Dispose();
}
