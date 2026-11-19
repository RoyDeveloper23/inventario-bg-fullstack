using InventarioBg.Domain.Entities;

namespace InventarioBg.Application.Abstractions.Persistence;

public interface IProductoRepository
{
    Task<Producto?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExisteSkuAsync(string codigoSku, Guid? excluirId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Producto>> BuscarAsync(string? busqueda, Guid? proveedorId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> ContarAsync(string? busqueda, Guid? proveedorId, CancellationToken cancellationToken = default);
    Task AgregarAsync(Producto producto, CancellationToken cancellationToken = default);
    Task GuardarAsync(CancellationToken cancellationToken = default);
}
