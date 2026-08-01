using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CoreSysHM.API.Security;
using CoreSysHM.Application.DTOs.Auditoria;
using CoreSysHM.Application.Interfaces.Services;
using CoreSysHM.Domain.Security;

namespace CoreSysHM.API.Controllers.Auditoria;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[HasPermission(Permissions.Seguridad.View)]
public class AuditoriaController : ControllerBase
{
    private readonly IAuditoriaService _auditoriaService;

    public AuditoriaController(IAuditoriaService auditoriaService)
    {
        _auditoriaService = auditoriaService;
    }

    [HttpGet]
    public async Task<IActionResult> Buscar([FromQuery] AuditoriaFiltroDto filtro)
    {
        var result = await _auditoriaService.BuscarAsync(filtro);
        return Ok(result);
    }
}
