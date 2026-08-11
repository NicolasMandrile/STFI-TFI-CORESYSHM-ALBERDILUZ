using CoreSysHM.Domain.Entities.Common;

namespace CoreSysHM.Domain.Entities.Stock;

public class Proveedor : BaseEntity
{
    public string RazonSocial { get; set; } = string.Empty;
    public string Cuit { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public string? Contacto { get; set; }

    public int? CondicionFiscalId { get; set; }
    public CondicionFiscal? CondicionFiscal { get; set; }

    /// <summary>Token de concurrencia optimista.</summary>
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
