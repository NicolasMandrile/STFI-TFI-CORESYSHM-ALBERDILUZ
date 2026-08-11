namespace CoreSysHM.Application.DTOs.Stock;

public class ProveedorDto
{
    public int Id { get; set; }
    public string RazonSocial { get; set; } = string.Empty;
    public string Cuit { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public string? Contacto { get; set; }
    public int? CondicionFiscalId { get; set; }
    public string? CondicionFiscalDescripcion { get; set; }

    /// <summary>% de campos relevantes para facturación completos (email, condición fiscal, domicilio, teléfono, contacto).</summary>
    public int Completitud { get; set; }
}

public class CreateProveedorDto
{
    public string RazonSocial { get; set; } = string.Empty;
    public string Cuit { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public string? Contacto { get; set; }
    public int? CondicionFiscalId { get; set; }
}
