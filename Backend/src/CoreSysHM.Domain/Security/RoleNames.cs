namespace CoreSysHM.Domain.Security;

/// <summary>
/// Los 3 únicos roles del sistema. No se agregan roles adicionales por fuera de estos.
/// </summary>
public static class RoleNames
{
    public const string Administrador = "Administrador";
    public const string Administrativo = "Administrativo";
    public const string Cliente = "Cliente";

    public static readonly IReadOnlyList<string> All = new[] { Administrador, Administrativo, Cliente };
}
