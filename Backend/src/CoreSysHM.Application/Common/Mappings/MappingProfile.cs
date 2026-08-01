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
        CreateMap<Proveedor, ProveedorDto>();
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
        CreateMap<Cliente, ClienteDto>();
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
            .ForMember(d => d.Estado, o => o.MapFrom(s => s.Estado.ToString()));
    }
}
