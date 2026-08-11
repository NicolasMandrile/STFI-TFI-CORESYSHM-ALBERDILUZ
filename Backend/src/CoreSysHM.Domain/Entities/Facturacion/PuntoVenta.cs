using CoreSysHM.Domain.Entities.Common;

namespace CoreSysHM.Domain.Entities.Facturacion;

public class PuntoVenta : BaseEntity
{
    public string Descripcion { get; set; } = string.Empty;

    public ICollection<Factura> Facturas { get; set; } = new List<Factura>();
}
