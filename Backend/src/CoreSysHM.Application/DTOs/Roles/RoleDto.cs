namespace CoreSysHM.Application.DTOs.Roles;

public class RoleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public bool IsSystem { get; set; }
    public bool IsSeeded { get; set; }
    public List<string> Permissions { get; set; } = new();
    public int CantidadUsuarios { get; set; }
    public DateTime CreatedAt { get; set; }
}
