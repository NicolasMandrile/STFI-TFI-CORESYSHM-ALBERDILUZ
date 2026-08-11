namespace CoreSysHM.Application.DTOs.Facturacion;

public class FacturaDto
{
    public int Id { get; set; }
    public string NumeroFactura { get; set; } = string.Empty;
    public DateTime FechaEmision { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public int ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public int VentaId { get; set; }
    public string NumeroVenta { get; set; } = string.Empty;
    public int TipoComprobanteId { get; set; }
    public string TipoComprobanteDescripcion { get; set; } = string.Empty;
    public int PuntoVentaId { get; set; }
    public string PuntoVentaDescripcion { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal Iva { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public List<DetalleFacturaDto> Detalles { get; set; } = new();
}

public class DetalleFacturaDto
{
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public int DetalleVentaId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Impuesto { get; set; }
    public decimal Descuento { get; set; }
    public decimal Subtotal { get; set; }
}

public class CreateFacturaDto
{
    public int VentaId { get; set; }
    public int TipoComprobanteId { get; set; }
    public int PuntoVentaId { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public string? Observaciones { get; set; }

    /// <summary>
    /// Clave que genera el cliente HTTP por cada intento de emisión (ej. un GUID). Reenviarla sin
    /// cambios en un reintento devuelve la factura ya creada en vez de duplicarla.
    /// </summary>
    public string IdempotencyKey { get; set; } = string.Empty;

    public List<CreateDetalleFacturaDto> Detalles { get; set; } = new();
}

public class CreateDetalleFacturaDto
{
    /// <summary>Línea de la Venta de origen que se está facturando (total o parcialmente).</summary>
    public int DetalleVentaId { get; set; }
    public int Cantidad { get; set; }

    /// <summary>Porcentaje de impuesto a aplicar sobre esta línea, ej. 21 = 21%.</summary>
    public decimal Impuesto { get; set; } = 21m;

    public decimal Descuento { get; set; }
}

/// <summary>Saldo pendiente de facturar de una línea de Venta (total vendido menos ya facturado, sin contar facturas anuladas).</summary>
public class SaldoFacturarLineaDto
{
    public int DetalleVentaId { get; set; }
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public decimal PrecioUnitario { get; set; }
    public int CantidadVenta { get; set; }
    public int CantidadFacturada { get; set; }
    public int CantidadPendiente { get; set; }
}

/// <summary>
/// Vista de una Venta confirmada con su saldo pendiente de facturar por línea -- fuente única
/// tanto para la pantalla "Nueva Factura" como para el reporte "Cartera por facturar" (Bloque 4).
/// </summary>
public class VentaFacturableDto
{
    public int VentaId { get; set; }
    public string NumeroVenta { get; set; } = string.Empty;
    public int ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public List<SaldoFacturarLineaDto> Lineas { get; set; } = new();
}

public class TipoComprobanteDto
{
    public int Id { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public bool AfectaStock { get; set; }
    public string SignoContable { get; set; } = string.Empty;
}

public class PuntoVentaDto
{
    public int Id { get; set; }
    public string Descripcion { get; set; } = string.Empty;
}
