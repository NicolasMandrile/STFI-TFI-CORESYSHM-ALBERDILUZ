using CoreSysHM.Domain.Enums;

namespace CoreSysHM.Application.DTOs.Ventas;

public class VentaDto
{
    public int Id { get; set; }
    public string NumeroVenta { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public int ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal Descuento { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = string.Empty;
    public List<DetalleVentaDto> Detalles { get; set; } = new();
}

public class CreateVentaDto
{
    public int ClienteId { get; set; }
    public decimal Descuento { get; set; }
    public string? Observaciones { get; set; }
    public List<CreateDetalleVentaDto> Detalles { get; set; } = new();
}

public class DetalleVentaDto
{
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}

public class CreateDetalleVentaDto
{
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
}

public class ClienteDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string? Dni { get; set; }
    public string? Cuit { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string? Localidad { get; set; }
    public int? CondicionFiscalId { get; set; }
    public string? CondicionFiscalDescripcion { get; set; }

    /// <summary>% de campos relevantes para facturación completos (email, condición fiscal, domicilio, teléfono, documento).</summary>
    public int Completitud { get; set; }
}

public class CreateClienteDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string? Dni { get; set; }
    public string? Cuit { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string? Localidad { get; set; }
    public int? CondicionFiscalId { get; set; }
}
