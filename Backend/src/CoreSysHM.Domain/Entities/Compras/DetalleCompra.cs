using CoreSysHM.Domain.Entities.Common;
using CoreSysHM.Domain.Entities.Stock;

namespace CoreSysHM.Domain.Entities.Compras;

public class DetalleCompra : BaseEntity
{
    public int CompraId { get; set; }
    public Compra Compra { get; set; } = null!;
    public int ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}
