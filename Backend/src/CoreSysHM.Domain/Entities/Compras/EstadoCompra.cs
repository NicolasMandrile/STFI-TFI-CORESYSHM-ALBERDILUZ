using CoreSysHM.Domain.Entities.Common;

namespace CoreSysHM.Domain.Entities.Compras;

public class EstadoCompra : BaseEntity
{
    public string Descripcion { get; set; } = string.Empty;
    public ICollection<Compra> Compras { get; set; } = new List<Compra>();
}
