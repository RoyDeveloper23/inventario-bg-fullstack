using InventarioBg.Application.Abstractions.Persistence;
using InventarioBg.Domain.Entities;
using InventarioBg.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventarioBg.Infrastructure.Repositories;

public sealed class ProductoInventarioRepository : IProductoInventarioRepository
{
    private readonly AppDbContext _context;

    public ProductoInventarioRepository(AppDbContext context) => _context = context;

    public Task<ProductoInventario?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.ProductosInventario
            .Include(x => x.Proveedor)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<bool> ExisteLoteAsync(
        Guid productoId,
        Guid proveedorId,
        string numeroLote,
        Guid? excluirId = null,
        CancellationToken cancellationToken = default)
    {
        var lote = numeroLote.Trim().ToUpperInvariant();
        return _context.ProductosInventario.AsNoTracking().AnyAsync(
            x => x.ProductoId == productoId &&
                 x.ProveedorId == proveedorId &&
                 x.NumeroLote.Trim().ToUpper() == lote &&
                 (!excluirId.HasValue || x.Id != excluirId.Value),
            cancellationToken);
    }

    public async Task<IReadOnlyList<ProductoInventario>> ObtenerPorProductoAsync(Guid productoId, CancellationToken cancellationToken = default)
    {
        return await _context.ProductosInventario
            .AsNoTracking()
            .Where(x => x.ProductoId == productoId)
            .Include(x => x.Proveedor)
            .OrderBy(x => x.FechaIngreso)
            .ToListAsync(cancellationToken);
    }

    public Task AgregarAsync(ProductoInventario inventario, CancellationToken cancellationToken = default)
    {
        return _context.ProductosInventario.AddAsync(inventario, cancellationToken).AsTask();
    }

    public Task EliminarAsync(ProductoInventario inventario, CancellationToken cancellationToken = default)
    {
        _context.ProductosInventario.Remove(inventario);
        return Task.CompletedTask;
    }

    public Task GuardarAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
