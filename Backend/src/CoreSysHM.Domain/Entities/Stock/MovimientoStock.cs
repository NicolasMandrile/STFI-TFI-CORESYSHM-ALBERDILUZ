using CoreSysHM.Domain.Entities.Common;

namespace CoreSysHM.Domain.Entities.Stock;

public class MovimientoStock : BaseEntity
{
    public int ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;
    public int Cantidad { get; set; }
    public string TipoMovimiento { get; set; } = string.Empty; // "ENTRADA" | "SALIDA"
    public string? Observacion { get; set; }
    public int StockAnterior { get; set; }
    public int StockPosterior { get; set; }
}
