using InventarioBg.Application.Contracts.Proveedores;

namespace InventarioBg.Application.Abstractions.Services;

public interface IProveedorService
{
    Task<IReadOnlyList<ProveedorDto>> ListarActivosAsync(CancellationToken cancellationToken = default);
}
