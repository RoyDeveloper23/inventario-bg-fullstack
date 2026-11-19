using InventarioBg.Application.Abstractions.Persistence;
using InventarioBg.Application.Abstractions.Services;
using InventarioBg.Application.Contracts.Inventario;
using InventarioBg.Application.Exceptions;
using InventarioBg.Domain.Entities;

namespace InventarioBg.Application.Services;

public sealed class ProductoInventarioService(
    IProductoRepository productoRepository,
    IProveedorRepository proveedorRepository,
    IProductoInventarioRepository inventarioRepository) : IProductoInventarioService
{
    public async Task<IReadOnlyList<ProductoInventarioDto>> ListarPorProductoAsync(
        Guid productoId,
        CancellationToken cancellationToken = default)
    {
        await ObtenerProductoActivoAsync(productoId, cancellationToken);
        var inventarios = await inventarioRepository.ObtenerPorProductoAsync(productoId, cancellationToken);
        return inventarios.Select(Mapear).ToArray();
    }

    public async Task<ProductoInventarioDto> CrearAsync(
        Guid productoId,
        CrearProductoInventarioDto solicitud,
        CancellationToken cancellationToken = default)
    {
        await ObtenerProductoActivoAsync(productoId, cancellationToken);
        if (solicitud.ProveedorId == Guid.Empty)
        {
            throw new ArgumentException("El proveedor es obligatorio.", nameof(solicitud));
        }

        var proveedor = await proveedorRepository.ObtenerPorIdAsync(solicitud.ProveedorId, cancellationToken);
        if (proveedor is not { Activo: true })
        {
            throw new RecursoNoEncontradoException("No se encontró el proveedor.");
        }

        if (await inventarioRepository.ExisteLoteAsync(
                productoId, solicitud.ProveedorId, solicitud.NumeroLote, cancellationToken: cancellationToken))
        {
            throw new ConflictoAplicacionException("Ya existe ese lote para el producto y proveedor.");
        }

        var inventario = new ProductoInventario(
            Guid.NewGuid(),
            productoId,
            solicitud.ProveedorId,
            solicitud.NumeroLote,
            solicitud.PrecioUnitario,
            solicitud.StockDisponible);
        await inventarioRepository.AgregarAsync(inventario, cancellationToken);
        await inventarioRepository.GuardarAsync(cancellationToken);
        return Mapear(inventario, proveedor.Nombre);
    }

    public async Task<ProductoInventarioDto> ActualizarAsync(
        Guid productoId,
        Guid inventarioId,
        ActualizarProductoInventarioDto solicitud,
        CancellationToken cancellationToken = default)
    {
        await ObtenerProductoActivoAsync(productoId, cancellationToken);
        var inventario = await ObtenerInventarioAsync(productoId, inventarioId, cancellationToken);
        inventario.Actualizar(solicitud.PrecioUnitario, solicitud.StockDisponible);
        await inventarioRepository.GuardarAsync(cancellationToken);
        return Mapear(inventario, inventario.Proveedor!.Nombre);
    }

    public async Task EliminarAsync(
        Guid productoId,
        Guid inventarioId,
        CancellationToken cancellationToken = default)
    {
        await ObtenerProductoActivoAsync(productoId, cancellationToken);
        var inventario = await ObtenerInventarioAsync(productoId, inventarioId, cancellationToken);
        if (inventario.StockDisponible != 0)
        {
            throw new ConflictoAplicacionException("Solo se pueden eliminar lotes sin existencias.");
        }

        await inventarioRepository.EliminarAsync(inventario, cancellationToken);
        await inventarioRepository.GuardarAsync(cancellationToken);
    }

    private async Task ObtenerProductoActivoAsync(Guid productoId, CancellationToken cancellationToken)
    {
        var producto = await productoRepository.ObtenerPorIdAsync(productoId, cancellationToken);
        if (producto is not { Activo: true })
        {
            throw new RecursoNoEncontradoException("No se encontró el producto.");
        }
    }

    private async Task<ProductoInventario> ObtenerInventarioAsync(
        Guid productoId,
        Guid inventarioId,
        CancellationToken cancellationToken)
    {
        var inventario = await inventarioRepository.ObtenerPorIdAsync(inventarioId, cancellationToken);
        return inventario is not null && inventario.ProductoId == productoId
            ? inventario
            : throw new RecursoNoEncontradoException("No se encontró el lote.");
    }

    private static ProductoInventarioDto Mapear(ProductoInventario inventario) =>
        Mapear(inventario, inventario.Proveedor!.Nombre);

    private static ProductoInventarioDto Mapear(ProductoInventario inventario, string proveedorNombre) => new(
        inventario.Id,
        inventario.ProductoId,
        inventario.ProveedorId,
        proveedorNombre,
        inventario.NumeroLote,
        inventario.PrecioUnitario,
        inventario.StockDisponible,
        inventario.FechaIngreso);
}
