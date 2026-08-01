using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CoreSysHM.API.Security;
using CoreSysHM.Application.DTOs.Stock;
using CoreSysHM.Application.Interfaces.Services;
using CoreSysHM.Domain.Security;

namespace CoreSysHM.API.Controllers.Stock;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductosController : ControllerBase
{
    private readonly IProductoService _productoService;

    public ProductosController(IProductoService productoService)
    {
        _productoService = productoService;
    }

    [HttpGet]
    [HasPermission(Permissions.Productos.View)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _productoService.GetAllAsync();
        if (result.Exitoso && result.Data is not null && User.IsInRole(RoleNames.Cliente))
            foreach (var producto in result.Data)
                OcultarDatosInternos(producto);
        return result.Exitoso ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id}")]
    [HasPermission(Permissions.Productos.View)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _productoService.GetByIdAsync(id);
        if (result.Exitoso && result.Data is not null && User.IsInRole(RoleNames.Cliente))
            OcultarDatosInternos(result.Data);
        return result.Exitoso ? Ok(result) : NotFound(result);
    }

    /// <summary>Rol Cliente: no debe ver costo ni proveedor -- son datos internos del negocio.</summary>
    private static void OcultarDatosInternos(ProductoDto producto)
    {
        producto.PrecioCompra = 0;
        producto.ProveedorId = null;
        producto.ProveedorNombre = string.Empty;
    }

    [HttpPost]
    [HasPermission(Permissions.Productos.Create)]
    public async Task<IActionResult> Create([FromBody] CreateProductoDto dto)
    {
        var result = await _productoService.CreateAsync(dto);
        return result.Exitoso ? CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result) : BadRequest(result);
    }

    [HttpPut("{id}")]
    [HasPermission(Permissions.Productos.Edit)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductoDto dto)
    {
        var result = await _productoService.UpdateAsync(id, dto);
        return result.Exitoso ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _productoService.DeleteAsync(id);
        return result.Exitoso ? Ok(result) : BadRequest(result);
    }

    [HttpGet("stock-bajo")]
    [HasPermission(Permissions.Productos.View)]
    public async Task<IActionResult> GetStockBajo()
    {
        // Reporte interno de reposición -- no forma parte del catálogo de autogestión del Cliente.
        if (User.IsInRole(RoleNames.Cliente)) return Forbid();

        var result = await _productoService.GetStockBajoAsync();
        return Ok(result);
    }

    [HttpPost("{id}/ajustar-stock")]
    [HasPermission(Permissions.Stock.Registrar)]
    public async Task<IActionResult> AjustarStock(int id, [FromBody] AjustarStockDto dto)
    {
        var result = await _productoService.AjustarStockAsync(id, dto.Cantidad, dto.TipoMovimiento, dto.Observacion);
        return result.Exitoso ? Ok(result) : BadRequest(result);
    }
}

public class AjustarStockDto
{
    public int Cantidad { get; set; }
    public string TipoMovimiento { get; set; } = string.Empty;
    public string? Observacion { get; set; }
}
