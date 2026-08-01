namespace CoreSysHM.Application.DTOs.Compras;

public class CompraDto
{
    public int Id { get; set; }
    public string NumeroCompra { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public int ProveedorId { get; set; }
    public string ProveedorNombre { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public int EstadoCompraId { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? RegistradoPorNombre { get; set; }
    public string? Observaciones { get; set; }
    public List<DetalleCompraDto> Detalles { get; set; } = new();
}

public class CreateCompraDto
{
    public int ProveedorId { get; set; }
    public string? Observaciones { get; set; }
    public List<CreateDetalleCompraDto> Detalles { get; set; } = new();
}

public class DetalleCompraDto
{
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}

public class CreateDetalleCompraDto
{
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
}
