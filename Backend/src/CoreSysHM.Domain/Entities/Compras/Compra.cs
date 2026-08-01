using CoreSysHM.Domain.Entities.Auth;
using CoreSysHM.Domain.Entities.Common;
using CoreSysHM.Domain.Entities.Stock;

namespace CoreSysHM.Domain.Entities.Compras;

public class Compra : BaseEntity
{
    public string NumeroCompra { get; set; } = string.Empty;
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public int ProveedorId { get; set; }
    public Proveedor Proveedor { get; set; } = null!;
    public decimal Total { get; set; }
    public int EstadoCompraId { get; set; }
    public EstadoCompra EstadoCompra { get; set; } = null!;
    public int? RegistradoPorId { get; set; }
    public ApplicationUser? RegistradoPor { get; set; }
    public string? Observaciones { get; set; }

    public ICollection<DetalleCompra> Detalles { get; set; } = new List<DetalleCompra>();
}
