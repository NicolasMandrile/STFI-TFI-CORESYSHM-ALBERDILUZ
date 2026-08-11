namespace CoreSysHM.Application.DTOs.Reportes;

public class FacturacionPorPeriodoDto
{
    public string Periodo { get; set; } = string.Empty;
    public int CantidadComprobantes { get; set; }
    public decimal Neto { get; set; }
    public decimal Iva { get; set; }
    public decimal Total { get; set; }
}

public class DesempenoClienteDto
{
    public int ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public int CantidadComprobantes { get; set; }
    public decimal MontoTotal { get; set; }
    public decimal TicketPromedio { get; set; }
}

public class DesempenoProductoDto
{
    public int ProductoId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int CantidadFacturada { get; set; }
    public decimal MontoTotal { get; set; }
}

/// <summary>KPIs de resumen (suma/conteo/promedio) para acompañar cualquiera de los reportes anteriores.</summary>
public class KpiResumenDto
{
    public int Cantidad { get; set; }
    public decimal Suma { get; set; }
    public decimal Promedio { get; set; }
}
