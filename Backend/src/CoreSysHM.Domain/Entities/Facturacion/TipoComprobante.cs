using CoreSysHM.Domain.Entities.Common;

namespace CoreSysHM.Domain.Entities.Facturacion;

public class TipoComprobante : BaseEntity
{
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>Si true, emitir/anular un comprobante de este tipo mueve stock.</summary>
    public bool AfectaStock { get; set; }

    /// <summary>"+" (factura: descuenta stock al emitir) o "-" (nota de crédito: lo repone).</summary>
    public string SignoContable { get; set; } = "+";

    public ICollection<Factura> Facturas { get; set; } = new List<Factura>();
}
