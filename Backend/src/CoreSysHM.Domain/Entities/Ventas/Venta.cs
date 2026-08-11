using CoreSysHM.Domain.Entities.Common;
using CoreSysHM.Domain.Entities.Facturacion;
using CoreSysHM.Domain.Enums;

namespace CoreSysHM.Domain.Entities.Ventas;

public class Venta : BaseEntity
{
    public string NumeroVenta { get; set; } = string.Empty;
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;
    public decimal Subtotal { get; set; }
    public decimal Descuento { get; set; }
    public decimal Total { get; set; }
    public EstadoVenta Estado { get; set; } = EstadoVenta.Pendiente;
    public string? Observaciones { get; set; }

    public ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();

    /// <summary>Facturas emitidas contra esta venta -- puede haber más de una (facturación parcial).</summary>
    public ICollection<Factura> Facturas { get; set; } = new List<Factura>();
}
