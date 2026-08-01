namespace CoreSysHM.Application.DTOs.Usuarios;

public class UsuarioDto
{
    public int Id { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? UltimoAcceso { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>Cliente de negocio vinculado (solo aplica a usuarios con rol Cliente). Null = sin vincular.</summary>
    public int? ClienteId { get; set; }
    public string? ClienteNombre { get; set; }
}
