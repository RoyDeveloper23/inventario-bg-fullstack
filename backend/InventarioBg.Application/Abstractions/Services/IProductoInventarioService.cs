using InventarioBg.Application.Contracts.Inventario;

namespace InventarioBg.Application.Abstractions.Services;

public interface IProductoInventarioService
{
    Task<IReadOnlyList<ProductoInventarioDto>> ListarPorProductoAsync(
        Guid productoId,
        CancellationToken cancellationToken = default);

    Task<ProductoInventarioDto> CrearAsync(
        Guid productoId,
        CrearProductoInventarioDto solicitud,
        CancellationToken cancellationToken = default);

    Task<ProductoInventarioDto> ActualizarAsync(
        Guid productoId,
        Guid inventarioId,
        ActualizarProductoInventarioDto solicitud,
        CancellationToken cancellationToken = default);

    Task EliminarAsync(Guid productoId, Guid inventarioId, CancellationToken cancellationToken = default);
}
