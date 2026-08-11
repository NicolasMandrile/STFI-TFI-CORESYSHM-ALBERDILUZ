using AutoMapper;
using CoreSysHM.Application.DTOs.Compras;
using CoreSysHM.Application.DTOs.Facturacion;
using CoreSysHM.Application.DTOs.Stock;
using CoreSysHM.Application.DTOs.Ventas;
using CoreSysHM.Domain.Entities.Compras;
using CoreSysHM.Domain.Entities.Facturacion;
using CoreSysHM.Domain.Entities.Stock;
using CoreSysHM.Domain.Entities.Ventas;

namespace CoreSysHM.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Stock
        CreateMap<Producto, ProductoDto>()
            .ForMember(d => d.CategoriaNombre, o => o.MapFrom(s => s.Categoria != null ? s.Categoria.Nombre : string.Empty))
            .ForMember(d => d.ProveedorNombre, o => o.MapFrom(s => s.Proveedor != null ? s.Proveedor.RazonSocial : string.Empty));
        CreateMap<CreateProductoDto, Producto>();
        CreateMap<UpdateProductoDto, Producto>();

        CreateMap<Categoria, CategoriaDto>();
        CreateMap<CreateCategoriaDto, Categoria>();

        CreateMap<MovimientoStock, MovimientoStockDto>()
            .ForMember(d => d.ProductoNombre, o => o.MapFrom(s => s.Producto != null ? s.Producto.Nombre : string.Empty))
            .ForMember(d => d.ProductoCodigo, o => o.MapFrom(s => s.Producto != null ? s.Producto.Codigo : string.Empty));

        // Proveedor
        CreateMap<Proveedor, ProveedorDto>()
            .ForMember(d => d.CondicionFiscalDescripcion, o => o.MapFrom(s => s.CondicionFiscal != null ? s.CondicionFiscal.Descripcion : null))
            .ForMember(d => d.Completitud, o => o.MapFrom(s => CalcularCompletitudProveedor(s)));
        CreateMap<CreateProveedorDto, Proveedor>();

        // Compras
        CreateMap<Compra, CompraDto>()
            .ForMember(d => d.ProveedorNombre, o => o.MapFrom(s => s.Proveedor != null ? s.Proveedor.RazonSocial : string.Empty))
            .ForMember(d => d.Estado,          o => o.MapFrom(s => s.EstadoCompra != null ? s.EstadoCompra.Descripcion : string.Empty))
            .ForMember(d => d.RegistradoPorNombre, o => o.MapFrom(s => s.RegistradoPor != null
                ? $"{s.RegistradoPor.Nombre} {s.RegistradoPor.Apellido}" : null));
        CreateMap<DetalleCompra, DetalleCompraDto>()
            .ForMember(d => d.ProductoNombre, o => o.MapFrom(s => s.Producto != null ? s.Producto.Nombre : string.Empty));

        // Ventas
        CreateMap<Cliente, ClienteDto>()
            .ForMember(d => d.CondicionFiscalDescripcion, o => o.MapFrom(s => s.CondicionFiscal != null ? s.CondicionFiscal.Descripcion : null))
            .ForMember(d => d.Completitud, o => o.MapFrom(s => CalcularCompletitudCliente(s)));
        CreateMap<CreateClienteDto, Cliente>();

        CreateMap<Venta, VentaDto>()
            .ForMember(d => d.ClienteNombre, o => o.MapFrom(s => s.Cliente != null ? $"{s.Cliente.Nombre} {s.Cliente.Apellido}" : string.Empty))
            .ForMember(d => d.Estado, o => o.MapFrom(s => s.Estado.ToString()));

        CreateMap<DetalleVenta, DetalleVentaDto>()
            .ForMember(d => d.ProductoNombre, o => o.MapFrom(s => s.Producto != null ? s.Producto.Nombre : string.Empty));

        // Facturacion
        CreateMap<Factura, FacturaDto>()
            .ForMember(d => d.ClienteNombre, o => o.MapFrom(s => s.Cliente != null ? $"{s.Cliente.Nombre} {s.Cliente.Apellido}" : string.Empty))
            .ForMember(d => d.NumeroVenta, o => o.MapFrom(s => s.Venta != null ? s.Venta.NumeroVenta : string.Empty))
            .ForMember(d => d.TipoComprobanteDescripcion, o => o.MapFrom(s => s.TipoComprobante != null ? s.TipoComprobante.Descripcion : string.Empty))
            .ForMember(d => d.PuntoVentaDescripcion, o => o.MapFrom(s => s.PuntoVenta != null ? s.PuntoVenta.Descripcion : string.Empty))
            .ForMember(d => d.Estado, o => o.MapFrom(s => s.Estado.ToString()));

        CreateMap<DetalleFactura, DetalleFacturaDto>()
            .ForMember(d => d.ProductoNombre, o => o.MapFrom(s => s.Producto != null ? s.Producto.Nombre : string.Empty));
    }

    // Campos considerados relevantes para poder facturar/contactar sin fricción -- Nombre/Apellido/
    // Cuit no cuentan porque ya son obligatorios desde el alta, no aportan a "qué tan completo" está.
    private static int CalcularCompletitudCliente(Cliente c)
    {
        var campos = new[]
        {
            !string.IsNullOrWhiteSpace(c.Email),
            c.CondicionFiscalId.HasValue,
            !string.IsNullOrWhiteSpace(c.Direccion),
            !string.IsNullOrWhiteSpace(c.Localidad),
            !string.IsNullOrWhiteSpace(c.Telefono),
            !string.IsNullOrWhiteSpace(c.Dni) || !string.IsNullOrWhiteSpace(c.Cuit),
        };
        return (int)Math.Round(100.0 * campos.Count(x => x) / campos.Length);
    }

    private static int CalcularCompletitudProveedor(Proveedor p)
    {
        var campos = new[]
        {
            !string.IsNullOrWhiteSpace(p.Email),
            p.CondicionFiscalId.HasValue,
            !string.IsNullOrWhiteSpace(p.Direccion),
            !string.IsNullOrWhiteSpace(p.Telefono),
            !string.IsNullOrWhiteSpace(p.Contacto),
        };
        return (int)Math.Round(100.0 * campos.Count(x => x) / campos.Length);
    }
}
