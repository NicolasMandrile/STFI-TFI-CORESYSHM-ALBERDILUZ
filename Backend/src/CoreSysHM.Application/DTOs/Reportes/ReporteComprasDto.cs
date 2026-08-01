namespace CoreSysHM.Application.DTOs.Reportes;

public class ComprasPorPeriodoDto
{
    public string  Periodo          { get; set; } = string.Empty;
    public int     CantidadCompras  { get; set; }
    public decimal TotalGastado     { get; set; }
}

public class RankingProveedorDto
{
    public int     ProveedorId      { get; set; }
    public string  RazonSocial      { get; set; } = string.Empty;
    public string  Cuit             { get; set; } = string.Empty;
    public int     CantidadCompras  { get; set; }
    public decimal MontoTotal       { get; set; }
    public decimal TicketPromedio   { get; set; }
}

public class ProductoMasCompradoDto
{
    public int     ProductoId       { get; set; }
    public string  Codigo           { get; set; } = string.Empty;
    public string  Nombre           { get; set; } = string.Empty;
    public int     CantidadComprada { get; set; }
    public decimal MontoTotal       { get; set; }
}

public class EvolucionPrecioCompraDto
{
    public string  Fecha            { get; set; } = string.Empty;
    public decimal PrecioUnitario   { get; set; }
    public string  NumeroCompra     { get; set; } = string.Empty;
}

public class SugerenciaReposicionDto
{
    public int     ProductoId           { get; set; }
    public string  Codigo               { get; set; } = string.Empty;
    public string  Nombre               { get; set; } = string.Empty;
    public int     StockActual          { get; set; }
    public int     StockMinimo          { get; set; }
    public int     Diferencia           { get; set; }
    public string  ProveedorNombre      { get; set; } = string.Empty;
    public decimal UltimoPrecioCompra   { get; set; }
}
