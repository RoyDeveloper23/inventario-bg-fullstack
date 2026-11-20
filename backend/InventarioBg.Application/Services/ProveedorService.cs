using InventarioBg.Application.Abstractions.Persistence;
using InventarioBg.Application.Abstractions.Services;
using InventarioBg.Application.Contracts.Proveedores;
using InventarioBg.Application.Exceptions;
using InventarioBg.Domain.Entities;

namespace InventarioBg.Application.Services;

public sealed class ProveedorService(IProveedorRepository proveedorRepository) : IProveedorService
{
    public async Task<IReadOnlyList<ProveedorDto>> ListarActivosAsync(CancellationToken cancellationToken = default)
    {
        var proveedores = await proveedorRepository.ObtenerActivosAsync(cancellationToken);
        return proveedores.Select(x => new ProveedorDto(x.Id, x.Nombre)).ToArray();
    }

    public async Task<ProveedorDto> CrearAsync(
        CrearProveedorDto solicitud,
        CancellationToken cancellationToken = default)
    {
        var nombre = solicitud.Nombre?.Trim() ?? string.Empty;
        if (nombre.Length is < 2 or > 200)
        {
            throw new ArgumentException("El nombre debe tener entre 2 y 200 caracteres.", nameof(solicitud));
        }
        if (await proveedorRepository.ExisteNombreAsync(nombre.ToUpperInvariant(), cancellationToken))
        {
            throw new ConflictoAplicacionException("Ya existe un proveedor con ese nombre.");
        }

        var proveedor = new Proveedor(Guid.NewGuid(), nombre);
        await proveedorRepository.AgregarAsync(proveedor, cancellationToken);
        await proveedorRepository.GuardarAsync(cancellationToken);
        return new ProveedorDto(proveedor.Id, proveedor.Nombre);
    }
}
