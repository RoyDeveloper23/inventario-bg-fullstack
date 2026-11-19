using InventarioBg.Domain.Entities;

namespace InventarioBg.Application.Abstractions.Persistence;

public interface IProductoInventarioRepository
{
    Task<ProductoInventario?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExisteLoteAsync(Guid productoId, Guid proveedorId, string numeroLote, Guid? excluirId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductoInventario>> ObtenerPorProductoAsync(Guid productoId, CancellationToken cancellationToken = default);
    Task AgregarAsync(ProductoInventario inventario, CancellationToken cancellationToken = default);
    Task EliminarAsync(ProductoInventario inventario, CancellationToken cancellationToken = default);
    Task GuardarAsync(CancellationToken cancellationToken = default);
}
