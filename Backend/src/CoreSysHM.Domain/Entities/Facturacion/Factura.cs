using CoreSysHM.Domain.Entities.Common;
using CoreSysHM.Domain.Entities.Ventas;
using CoreSysHM.Domain.Enums;

namespace CoreSysHM.Domain.Entities.Facturacion;

public class Factura : BaseEntity
{
    public string NumeroFactura { get; set; } = string.Empty;
    public DateTime FechaEmision { get; set; } = DateTime.UtcNow;
    public DateTime? FechaVencimiento { get; set; }
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;
    public int VentaId { get; set; }
    public Venta Venta { get; set; } = null!;
    public decimal Subtotal { get; set; }
    public decimal Iva { get; set; }
    public decimal Total { get; set; }
    public EstadoFactura Estado { get; set; } = EstadoFactura.Emitida;
    public string? Observaciones { get; set; }
}
