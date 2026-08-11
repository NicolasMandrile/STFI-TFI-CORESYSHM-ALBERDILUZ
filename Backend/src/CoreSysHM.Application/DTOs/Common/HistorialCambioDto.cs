namespace CoreSysHM.Application.DTOs.Common;

public class HistorialCambioDto
{
    public DateTime Fecha { get; set; }
    public string Accion { get; set; } = string.Empty;
    public string? UsuarioNombre { get; set; }
    public string? Detalle { get; set; }
}
