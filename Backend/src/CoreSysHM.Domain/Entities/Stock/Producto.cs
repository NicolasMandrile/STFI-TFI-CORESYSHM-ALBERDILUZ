using CoreSysHM.Domain.Entities.Common;

namespace CoreSysHM.Domain.Entities.Stock;

public class Producto : BaseEntity
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal PrecioCompra { get; set; }
    public decimal PrecioVenta { get; set; }
    public int StockActual { get; set; }
    public int StockMinimo { get; set; }
    public int CategoriaId { get; set; }
    public Categoria Categoria { get; set; } = null!;
    public int? ProveedorId { get; set; }
    public Proveedor? Proveedor { get; set; }

    public ICollection<MovimientoStock> Movimientos { get; set; } = new List<MovimientoStock>();
}
