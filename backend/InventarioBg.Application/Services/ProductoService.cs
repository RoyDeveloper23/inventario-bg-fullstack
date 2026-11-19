using InventarioBg.Application.Abstractions.Persistence;
using InventarioBg.Application.Abstractions.Services;
using InventarioBg.Application.Contracts.Common;
using InventarioBg.Application.Contracts.Productos;
using InventarioBg.Application.Exceptions;
using InventarioBg.Domain.Entities;

namespace InventarioBg.Application.Services;

public sealed class ProductoService(IProductoRepository productoRepository) : IProductoService
{
    public async Task<PagedResultDto<ProductoListItemDto>> ListarAsync(
        string? busqueda,
        Guid? proveedorId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (page < 1 || pageSize is < 1 or > 100 || (long)(page - 1) * pageSize > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(page), "Los parámetros de paginación no son válidos.");
        }

        var productos = await productoRepository.BuscarAsync(
            busqueda, proveedorId, page, pageSize, cancellationToken);
        var totalCount = await productoRepository.ContarAsync(busqueda, proveedorId, cancellationToken);
        var items = productos.Select(x => new ProductoListItemDto(
            x.Id,
            x.CodigoSku,
            x.Nombre,
            x.Descripcion,
            x.Categoria,
            x.Activo,
            x.Inventarios.Sum(i => (long)i.StockDisponible))).ToArray();

        return new PagedResultDto<ProductoListItemDto>(items, totalCount, page, pageSize);
    }

    public async Task<ProductoDto> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var producto = await ObtenerProductoAsync(id, cancellationToken);
        return Mapear(producto);
    }

    public async Task<ProductoDto> CrearAsync(
        ProductoRequestDto solicitud,
        CancellationToken cancellationToken = default)
    {
        if (await productoRepository.ExisteSkuAsync(solicitud.CodigoSku, cancellationToken: cancellationToken))
        {
            throw new ConflictoAplicacionException("Ya existe un producto con ese código SKU.");
        }

        var producto = new Producto(
            Guid.NewGuid(),
            solicitud.CodigoSku,
            solicitud.Nombre,
            solicitud.Descripcion,
            solicitud.Categoria);
        await productoRepository.AgregarAsync(producto, cancellationToken);
        await productoRepository.GuardarAsync(cancellationToken);
        return Mapear(producto);
    }

    public async Task<ProductoDto> ActualizarAsync(
        Guid id,
        ProductoRequestDto solicitud,
        CancellationToken cancellationToken = default)
    {
        var producto = await ObtenerProductoAsync(id, cancellationToken);
        if (await productoRepository.ExisteSkuAsync(solicitud.CodigoSku, id, cancellationToken))
        {
            throw new ConflictoAplicacionException("Ya existe un producto con ese código SKU.");
        }

        producto.Actualizar(solicitud.CodigoSku, solicitud.Nombre, solicitud.Descripcion, solicitud.Categoria);
        await productoRepository.GuardarAsync(cancellationToken);
        return Mapear(producto);
    }

    public async Task DesactivarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var producto = await ObtenerProductoAsync(id, cancellationToken);
        producto.Desactivar();
        await productoRepository.GuardarAsync(cancellationToken);
    }

    private async Task<Producto> ObtenerProductoAsync(Guid id, CancellationToken cancellationToken)
    {
        var producto = await productoRepository.ObtenerPorIdAsync(id, cancellationToken);
        return producto is { Activo: true }
            ? producto
            : throw new RecursoNoEncontradoException("No se encontró el producto.");
    }

    private static ProductoDto Mapear(Producto producto) => new(
        producto.Id,
        producto.CodigoSku,
        producto.Nombre,
        producto.Descripcion,
        producto.Categoria,
        producto.Activo);
}
