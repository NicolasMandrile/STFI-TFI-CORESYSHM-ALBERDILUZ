namespace CoreSysHM.Application.DTOs.Auditoria;

public class AuditoriaAccesoDto
{
    public long Id { get; set; }
    public int? UsuarioId { get; set; }
    public string? UsuarioNombre { get; set; }
    public string? UsuarioEmail { get; set; }
    public string? RolSnapshot { get; set; }
    public string Accion { get; set; } = string.Empty;
    public string? Ip { get; set; }
    public string? UserAgent { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Detalle { get; set; }
}
