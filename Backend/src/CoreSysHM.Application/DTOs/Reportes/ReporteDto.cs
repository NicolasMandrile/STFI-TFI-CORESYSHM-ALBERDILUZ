namespace CoreSysHM.Application.DTOs.Reportes;

public class ProductoMasVendidoDto
{
    public int    ProductoId       { get; set; }
    public string Codigo           { get; set; } = string.Empty;
    public string Nombre           { get; set; } = string.Empty;
    public int    CantidadVendida  { get; set; }
    public decimal TotalFacturado  { get; set; }
}

public class VentasPorPeriodoDto
{
    public string  Periodo         { get; set; } = string.Empty;
    public int     CantidadVentas  { get; set; }
    public decimal TotalFacturado  { get; set; }
}

public class RankingClienteDto
{
    public int     ClienteId        { get; set; }
    public string  Nombre           { get; set; } = string.Empty;
    public string  Apellido         { get; set; } = string.Empty;
    public int     CantidadCompras  { get; set; }
    public decimal MontoTotal       { get; set; }
    public decimal TicketPromedio   { get; set; }
}

public class TicketPromedioDto
{
    public int     CantidadVentas  { get; set; }
    public decimal TotalFacturado  { get; set; }
    public decimal TicketPromedio  { get; set; }
}

public class MargenProductoDto
{
    public int     ProductoId        { get; set; }
    public string  Codigo            { get; set; } = string.Empty;
    public string  Nombre            { get; set; } = string.Empty;
    public int     CantidadVendida   { get; set; }
    public decimal IngresoTotal      { get; set; }
    public decimal CostoTotal        { get; set; }
    public decimal MargenTotal       { get; set; }
    public decimal MargenPorcentual  { get; set; }
}
