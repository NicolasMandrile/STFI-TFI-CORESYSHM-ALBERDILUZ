namespace CoreSysHM.Application.DTOs.Usuarios;

public class CreateUsuarioDto
{
    public string NombreUsuario { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;

    /// <summary>Solo aplica cuando Rol == "Cliente": vincula este login al Cliente de negocio indicado.</summary>
    public int? ClienteId { get; set; }
}
