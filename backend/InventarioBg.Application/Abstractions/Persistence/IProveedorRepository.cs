using InventarioBg.Domain.Entities;

namespace InventarioBg.Application.Abstractions.Persistence;

public interface IProveedorRepository
{
    Task<IReadOnlyList<Proveedor>> ObtenerActivosAsync(CancellationToken cancellationToken = default);
    Task<Proveedor?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExisteNombreAsync(string nombre, CancellationToken cancellationToken = default);
    Task AgregarAsync(Proveedor proveedor, CancellationToken cancellationToken = default);
    Task GuardarAsync(CancellationToken cancellationToken = default);
}
