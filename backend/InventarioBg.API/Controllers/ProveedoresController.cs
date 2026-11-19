using InventarioBg.Application.Abstractions.Services;
using InventarioBg.Application.Contracts.Common;
using InventarioBg.Application.Contracts.Proveedores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventarioBg.API.Controllers;

[ApiController]
[Authorize]
[Route("api/proveedores")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public sealed class ProveedoresController(IProveedorService proveedorService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ItemsResultDto<ProveedorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ItemsResultDto<ProveedorDto>>> Listar(CancellationToken cancellationToken)
    {
        IReadOnlyList<ProveedorDto> items = await proveedorService.ListarActivosAsync(cancellationToken);
        return Ok(new ItemsResultDto<ProveedorDto>(items));
    }
}
