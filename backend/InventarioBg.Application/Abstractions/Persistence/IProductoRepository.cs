using InventarioBg.Domain.Entities;

namespace InventarioBg.Application.Abstractions.Persistence;

public interface IProductoRepository
{
    Task<Producto?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExisteSkuAsync(string codigoSku, Guid? excluirId = null, CancellationToken cancellationToken = default);
    Task<bool> ExisteNombreAsync(
        string nombreNormalizado,
        Guid? excluirId = null,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Producto>> BuscarAsync(
        string? busqueda,
        Guid? proveedorId,
        string? categoria,
        bool? activo,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<int> ContarAsync(
        string? busqueda,
        Guid? proveedorId,
        string? categoria,
        bool? activo,
        CancellationToken cancellationToken = default);
    Task AgregarAsync(Producto producto, CancellationToken cancellationToken = default);
    void Eliminar(Producto producto);
    Task GuardarAsync(CancellationToken cancellationToken = default);
}
