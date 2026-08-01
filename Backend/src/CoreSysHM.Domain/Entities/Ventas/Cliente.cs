using CoreSysHM.Domain.Entities.Auth;
using CoreSysHM.Domain.Entities.Common;
using CoreSysHM.Domain.Entities.Facturacion;

namespace CoreSysHM.Domain.Entities.Ventas;

public class Cliente : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string? Dni { get; set; }
    public string? Cuit { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string? Localidad { get; set; }

    /// <summary>
    /// Login (AspNetUsers) vinculado a este cliente de negocio -- opcional. Cuando está seteado,
    /// permite que un usuario con rol "Cliente" vea únicamente sus propias ventas (portal de cliente).
    /// </summary>
    public int? UserId { get; set; }
    public ApplicationUser? Usuario { get; set; }

    public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
    public ICollection<Factura> Facturas { get; set; } = new List<Factura>();
}
