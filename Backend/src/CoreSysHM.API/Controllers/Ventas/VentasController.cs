using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CoreSysHM.API.Security;
using CoreSysHM.Application.Common.Wrappers;
using CoreSysHM.Application.DTOs.Ventas;
using CoreSysHM.Application.Interfaces.Services;
using CoreSysHM.Domain.Security;

namespace CoreSysHM.API.Controllers.Ventas;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VentasController : ControllerBase
{
    /// <summary>Sentinel: usuario con rol Cliente sin Cliente de negocio vinculado -- no debe ver ninguna venta.</summary>
    private const int SinClienteVinculado = -1;

    private readonly IVentaService _ventaService;

    public VentasController(IVentaService ventaService)
    {
        _ventaService = ventaService;
    }

    [HttpGet]
    [HasPermission(Permissions.Ventas.View)]
    public async Task<IActionResult> GetAll()
    {
        var clienteIdFiltro = await ObtenerFiltroClienteAsync();
        if (clienteIdFiltro == SinClienteVinculado)
            return Ok(ApiResponse<IEnumerable<VentaDto>>.Success(Enumerable.Empty<VentaDto>()));

        var result = await _ventaService.GetAllAsync(clienteIdFiltro);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [HasPermission(Permissions.Ventas.View)]
    public async Task<IActionResult> GetById(int id)
    {
        var clienteIdFiltro = await ObtenerFiltroClienteAsync();
        if (clienteIdFiltro == SinClienteVinculado)
            return NotFound(ApiResponse<VentaDto>.Failure("Venta no encontrada."));

        var result = await _ventaService.GetByIdAsync(id, clienteIdFiltro);
        return result.Exitoso ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [HasPermission(Permissions.Ventas.Create)]
    public async Task<IActionResult> Create([FromBody] CreateVentaDto dto)
    {
        // Rol Cliente: el ClienteId del body NO es confiable (permitiría registrar ventas a
        // nombre de otro cliente). Se pisa siempre con el cliente de negocio vinculado al login.
        if (User.IsInRole(RoleNames.Cliente))
        {
            var clienteIdPropio = await ObtenerFiltroClienteAsync();
            if (clienteIdPropio is null or SinClienteVinculado)
                return Forbid();
            dto.ClienteId = clienteIdPropio.Value;
        }

        var result = await _ventaService.CreateAsync(dto);
        return result.Exitoso ? CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result) : BadRequest(result);
    }

    [HttpPost("{id}/confirmar")]
    [Authorize(Roles = "Administrador,Administrativo")]
    public async Task<IActionResult> Confirmar(int id)
    {
        var result = await _ventaService.ConfirmarAsync(id);
        return result.Exitoso ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/anular")]
    [Authorize(Roles = "Administrador,Administrativo")]
    public async Task<IActionResult> Anular(int id)
    {
        var result = await _ventaService.AnularAsync(id);
        return result.Exitoso ? Ok(result) : BadRequest(result);
    }

    [HttpGet("clientes")]
    [HasPermission(Permissions.Clientes.View)]
    public async Task<IActionResult> GetClientes()
    {
        var result = await _ventaService.GetClientesAsync();
        return Ok(result);
    }

    [HttpPost("clientes")]
    [HasPermission(Permissions.Clientes.Create)]
    public async Task<IActionResult> CreateCliente([FromBody] CreateClienteDto dto)
    {
        var result = await _ventaService.CreateClienteAsync(dto);
        return result.Exitoso ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Staff (Administrador/Administrativo) -> null (sin filtro, ve todas las ventas).
    /// Rol Cliente -> el Id de SU Cliente de negocio vinculado, o <see cref="SinClienteVinculado"/>
    /// si el login todavía no fue vinculado a ningún Cliente (no debe ver nada).
    /// </summary>
    private async Task<int?> ObtenerFiltroClienteAsync()
    {
        if (!User.IsInRole(RoleNames.Cliente)) return null;

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!int.TryParse(userIdClaim, out var userId)) return SinClienteVinculado;

        var clienteId = await _ventaService.GetClienteIdByUserIdAsync(userId);
        return clienteId ?? SinClienteVinculado;
    }
}
