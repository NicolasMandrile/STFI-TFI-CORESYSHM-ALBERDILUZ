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

    /// <summary>Venta de origen. Ya NO es 1 a 1: una Venta puede tener varias Facturas (facturación parcial).</summary>
    public int VentaId { get; set; }
    public Venta Venta { get; set; } = null!;

    public int TipoComprobanteId { get; set; }
    public TipoComprobante TipoComprobante { get; set; } = null!;
    public int PuntoVentaId { get; set; }
    public PuntoVenta PuntoVenta { get; set; } = null!;

    public decimal Subtotal { get; set; }
    public decimal Iva { get; set; }
    public decimal Total { get; set; }
    public EstadoFactura Estado { get; set; } = EstadoFactura.Emitida;
    public string? Observaciones { get; set; }

    /// <summary>Clave provista por el cliente HTTP para evitar duplicados ante reintentos.</summary>
    public string? IdempotencyKey { get; set; }

    /// <summary>Token de concurrencia optimista.</summary>
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public ICollection<DetalleFactura> Detalles { get; set; } = new List<DetalleFactura>();
}
