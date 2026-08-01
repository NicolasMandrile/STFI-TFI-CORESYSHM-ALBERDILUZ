namespace CoreSysHM.Application.DTOs.Stock;

public class MovimientoStockDto
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public string ProductoCodigo { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public string TipoMovimiento { get; set; } = string.Empty;
    public string? Observacion { get; set; }
    public int StockAnterior { get; set; }
    public int StockPosterior { get; set; }
    public DateTime FechaCreacion { get; set; }
}

public class CreateMovimientoStockDto
{
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
    public string TipoMovimiento { get; set; } = string.Empty; // ENTRADA | SALIDA | AJUSTE | PERDIDA | RECUENTO
    public string? Observacion { get; set; }
}
