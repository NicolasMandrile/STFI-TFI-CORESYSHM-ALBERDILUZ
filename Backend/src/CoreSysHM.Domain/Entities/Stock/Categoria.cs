using CoreSysHM.Domain.Entities.Common;

namespace CoreSysHM.Domain.Entities.Stock;

public class Categoria : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }

    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
