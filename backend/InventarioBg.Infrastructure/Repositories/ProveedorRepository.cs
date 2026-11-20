using InventarioBg.Application.Abstractions.Persistence;
using InventarioBg.Domain.Entities;
using InventarioBg.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventarioBg.Infrastructure.Repositories;

public sealed class ProveedorRepository : IProveedorRepository
{
    private readonly AppDbContext _context;

    public ProveedorRepository(AppDbContext context) => _context = context;

    public async Task<IReadOnlyList<Proveedor>> ObtenerActivosAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Proveedores
            .AsNoTracking()
            .Where(x => x.Activo)
            .OrderBy(x => x.Nombre)
            .ToListAsync(cancellationToken);
    }

    public Task<Proveedor?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Proveedores
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<bool> ExisteNombreAsync(string nombre, CancellationToken cancellationToken = default)
    {
        var normalizado = nombre.Trim().ToUpperInvariant();
        return _context.Proveedores.AsNoTracking().AnyAsync(
            x => x.Nombre.Trim().ToUpper() == normalizado,
            cancellationToken);
    }

    public Task AgregarAsync(Proveedor proveedor, CancellationToken cancellationToken = default)
    {
        return _context.Proveedores.AddAsync(proveedor, cancellationToken).AsTask();
    }

    public Task GuardarAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
