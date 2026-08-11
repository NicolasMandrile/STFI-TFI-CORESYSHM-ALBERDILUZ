using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoreSysHM.Application.Common.Wrappers;
using CoreSysHM.Application.DTOs.Common;
using CoreSysHM.Infrastructure.Data;

namespace CoreSysHM.API.Controllers.Comun;

/// <summary>Catálogo de solo lectura, compartido por los formularios de Cliente y Proveedor.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CondicionesFiscalesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CondicionesFiscalesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var condiciones = await _context.CondicionesFiscales
            .Where(c => c.Activo)
            .OrderBy(c => c.Descripcion)
            .Select(c => new CondicionFiscalDto { Id = c.Id, Descripcion = c.Descripcion })
            .ToListAsync();
        return Ok(ApiResponse<IEnumerable<CondicionFiscalDto>>.Success(condiciones));
    }
}
