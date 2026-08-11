using CoreSysHM.Domain.Entities.Common;
using CoreSysHM.Domain.Entities.Stock;
using CoreSysHM.Domain.Entities.Ventas;

namespace CoreSysHM.Domain.Entities.Facturacion;

public class DetalleFactura : BaseEntity
{
    public int FacturaId { get; set; }
    public Factura Factura { get; set; } = null!;
    public int ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;

    /// <summary>
    /// Línea de la Venta de origen que esta línea de factura está cubriendo -- es lo que permite
    /// calcular cuánto de cada DetalleVenta ya fue facturado (facturación parcial) sin ambigüedad.
    /// </summary>
    public int DetalleVentaId { get; set; }
    public DetalleVenta DetalleVenta { get; set; } = null!;

    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }

    /// <summary>Porcentaje de impuesto (ej. 21.00 = 21%).</summary>
    public decimal Impuesto { get; set; }

    public decimal Descuento { get; set; }
    public decimal Subtotal { get; set; }
}
