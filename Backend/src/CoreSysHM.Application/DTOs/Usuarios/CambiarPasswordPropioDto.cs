namespace CoreSysHM.Application.DTOs.Usuarios;

public class CambiarPasswordPropioDto
{
    public string PasswordActual { get; set; } = string.Empty;
    public string PasswordNueva { get; set; } = string.Empty;
}
