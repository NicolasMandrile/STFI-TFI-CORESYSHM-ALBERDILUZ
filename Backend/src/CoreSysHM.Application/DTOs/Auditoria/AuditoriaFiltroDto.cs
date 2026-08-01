namespace CoreSysHM.Application.DTOs.Auditoria;

public class AuditoriaFiltroDto
{
    public int? UsuarioId { get; set; }
    public string? Accion { get; set; }
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    public int Pagina { get; set; } = 1;
    public int TamanoPagina { get; set; } = 20;
}
