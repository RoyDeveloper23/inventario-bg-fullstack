using InventarioBg.Domain.Entities;

namespace InventarioBg.Application.Abstractions.Persistence;

public interface IProveedorRepository
{
    Task<IReadOnlyList<Proveedor>> ObtenerActivosAsync(CancellationToken cancellationToken = default);
    Task<Proveedor?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
}
