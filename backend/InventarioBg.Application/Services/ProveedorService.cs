using InventarioBg.Application.Abstractions.Persistence;
using InventarioBg.Application.Abstractions.Services;
using InventarioBg.Application.Contracts.Proveedores;

namespace InventarioBg.Application.Services;

public sealed class ProveedorService(IProveedorRepository proveedorRepository) : IProveedorService
{
    public async Task<IReadOnlyList<ProveedorDto>> ListarActivosAsync(CancellationToken cancellationToken = default)
    {
        var proveedores = await proveedorRepository.ObtenerActivosAsync(cancellationToken);
        return proveedores.Select(x => new ProveedorDto(x.Id, x.Nombre)).ToArray();
    }
}
