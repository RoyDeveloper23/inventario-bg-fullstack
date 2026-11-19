using System.ComponentModel.DataAnnotations;
using InventarioBg.Application.Abstractions.Services;
using InventarioBg.Application.Contracts.Common;
using InventarioBg.Application.Contracts.Inventario;
using InventarioBg.Application.Contracts.Productos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventarioBg.API.Controllers;

[ApiController]
[Authorize]
[Route("api/productos")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public sealed class ProductosController(
    IProductoService productoService,
    IProductoInventarioService inventarioService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<ProductoListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResultDto<ProductoListItemDto>>> Listar(
        [FromQuery] string? busqueda,
        [FromQuery] Guid? proveedorId,
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return Ok(await productoService.ListarAsync(busqueda, proveedorId, page, pageSize, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductoDto>> ObtenerPorId(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await productoService.ObtenerPorIdAsync(id, cancellationToken));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProductoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductoDto>> Crear(
        [FromBody] ProductoRequestDto solicitud,
        CancellationToken cancellationToken)
    {
        var producto = await productoService.CrearAsync(solicitud, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = producto.Id }, producto);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProductoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductoDto>> Actualizar(
        Guid id,
        [FromBody] ProductoRequestDto solicitud,
        CancellationToken cancellationToken)
    {
        return Ok(await productoService.ActualizarAsync(id, solicitud, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desactivar(Guid id, CancellationToken cancellationToken)
    {
        await productoService.DesactivarAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("{productoId:guid}/inventarios")]
    [ProducesResponseType(typeof(ItemsResultDto<ProductoInventarioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ItemsResultDto<ProductoInventarioDto>>> ListarInventarios(
        Guid productoId,
        CancellationToken cancellationToken)
    {
        var items = await inventarioService.ListarPorProductoAsync(productoId, cancellationToken);
        return Ok(new ItemsResultDto<ProductoInventarioDto>(items));
    }

    [HttpPost("{productoId:guid}/inventarios")]
    [ProducesResponseType(typeof(ProductoInventarioDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductoInventarioDto>> CrearInventario(
        Guid productoId,
        [FromBody] CrearProductoInventarioDto solicitud,
        CancellationToken cancellationToken)
    {
        var inventario = await inventarioService.CrearAsync(productoId, solicitud, cancellationToken);
        return Created($"/api/productos/{productoId}/inventarios/{inventario.Id}", inventario);
    }

    [HttpPut("{productoId:guid}/inventarios/{inventarioId:guid}")]
    [ProducesResponseType(typeof(ProductoInventarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductoInventarioDto>> ActualizarInventario(
        Guid productoId,
        Guid inventarioId,
        [FromBody] ActualizarProductoInventarioDto solicitud,
        CancellationToken cancellationToken)
    {
        return Ok(await inventarioService.ActualizarAsync(productoId, inventarioId, solicitud, cancellationToken));
    }

    [HttpDelete("{productoId:guid}/inventarios/{inventarioId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> EliminarInventario(
        Guid productoId,
        Guid inventarioId,
        CancellationToken cancellationToken)
    {
        await inventarioService.EliminarAsync(productoId, inventarioId, cancellationToken);
        return NoContent();
    }
}
