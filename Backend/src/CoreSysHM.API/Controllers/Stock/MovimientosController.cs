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
public class MovimientosController : ControllerBase
{
    private readonly IMovimientoStockService _movimientoService;

    public MovimientosController(IMovimientoStockService movimientoService)
    {
        _movimientoService = movimientoService;
    }

    [HttpGet]
    [HasPermission(Permissions.Stock.View)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _movimientoService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("producto/{productoId}")]
    [HasPermission(Permissions.Stock.View)]
    public async Task<IActionResult> GetByProducto(int productoId)
    {
        var result = await _movimientoService.GetByProductoAsync(productoId);
        return Ok(result);
    }

    [HttpPost]
    [HasPermission(Permissions.Stock.Registrar)]
    public async Task<IActionResult> Registrar([FromBody] CreateMovimientoStockDto dto)
    {
        var result = await _movimientoService.RegistrarMovimientoAsync(dto);
        return result.Exitoso ? Ok(result) : BadRequest(result);
    }
}
