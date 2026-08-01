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
public class CategoriasController : ControllerBase
{
    private readonly ICategoriaService _categoriaService;

    public CategoriasController(ICategoriaService categoriaService)
    {
        _categoriaService = categoriaService;
    }

    [HttpGet]
    [HasPermission(Permissions.Categorias.View)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _categoriaService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    [HasPermission(Permissions.Categorias.View)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _categoriaService.GetByIdAsync(id);
        return result.Exitoso ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [HasPermission(Permissions.Categorias.Create)]
    public async Task<IActionResult> Create([FromBody] CreateCategoriaDto dto)
    {
        var result = await _categoriaService.CreateAsync(dto);
        return result.Exitoso ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id}")]
    [HasPermission(Permissions.Categorias.Edit)]
    public async Task<IActionResult> Update(int id, [FromBody] CreateCategoriaDto dto)
    {
        var result = await _categoriaService.UpdateAsync(id, dto);
        return result.Exitoso ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _categoriaService.DeleteAsync(id);
        return result.Exitoso ? Ok(result) : BadRequest(result);
    }
}
