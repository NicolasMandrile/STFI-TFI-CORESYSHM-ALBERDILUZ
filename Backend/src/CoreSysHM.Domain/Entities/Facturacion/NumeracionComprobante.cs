using CoreSysHM.Domain.Entities.Common;

namespace CoreSysHM.Domain.Entities.Facturacion;

/// <summary>
/// Contador correlativo por (PuntoVenta, TipoComprobante). Se incrementa con un UPDATE atómico
/// (ver FacturaService) para garantizar numeración única aun con emisiones concurrentes.
/// </summary>
public class NumeracionComprobante : BaseEntity
{
    public int PuntoVentaId { get; set; }
    public PuntoVenta PuntoVenta { get; set; } = null!;
    public int TipoComprobanteId { get; set; }
    public TipoComprobante TipoComprobante { get; set; } = null!;
    public int UltimoNumero { get; set; }
}
