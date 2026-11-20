using InventarioBg.Application.Abstractions.Persistence;
using InventarioBg.Domain.Entities;
using InventarioBg.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventarioBg.Infrastructure.Repositories;

public sealed class ProductoRepository : IProductoRepository
{
    private readonly AppDbContext _context;

    public ProductoRepository(AppDbContext context) => _context = context;

    public Task<Producto?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Productos
            .Include(x => x.Inventarios)
            .ThenInclude(x => x.Proveedor)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<bool> ExisteSkuAsync(string codigoSku, Guid? excluirId = null, CancellationToken cancellationToken = default)
    {
        var normalizado = codigoSku.Trim().ToUpperInvariant();
        return _context.Productos.AsNoTracking().AnyAsync(
            x => x.CodigoSku == normalizado && (!excluirId.HasValue || x.Id != excluirId.Value),
            cancellationToken);
    }

    public Task<bool> ExisteNombreAsync(
        string nombreNormalizado,
        Guid? excluirId = null,
        CancellationToken cancellationToken = default)
    {
        return _context.Productos.AsNoTracking().AnyAsync(
            x => x.Nombre.Trim().ToLower() == nombreNormalizado &&
                (!excluirId.HasValue || x.Id != excluirId.Value),
            cancellationToken);
    }

    public async Task<IReadOnlyList<Producto>> BuscarAsync(
        string? busqueda,
        Guid? proveedorId,
        string? categoria,
        bool? activo,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var consulta = Filtrar(busqueda, proveedorId, categoria, activo);
        return await consulta
            .AsNoTracking()
            .OrderBy(x => x.Nombre)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(x => x.Inventarios)
            .ThenInclude(x => x.Proveedor)
            .ToListAsync(cancellationToken);
    }

    public Task<int> ContarAsync(
        string? busqueda,
        Guid? proveedorId,
        string? categoria,
        bool? activo,
        CancellationToken cancellationToken = default)
    {
        return Filtrar(busqueda, proveedorId, categoria, activo).CountAsync(cancellationToken);
    }

    public Task AgregarAsync(Producto producto, CancellationToken cancellationToken = default)
    {
        return _context.Productos.AddAsync(producto, cancellationToken).AsTask();
    }

    public void Eliminar(Producto producto)
    {
        _context.Productos.Remove(producto);
    }

    public Task GuardarAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<Producto> Filtrar(
        string? busqueda,
        Guid? proveedorId,
        string? categoria,
        bool? activo)
    {
        var consulta = _context.Productos.AsQueryable();
        if (activo.HasValue)
        {
            consulta = consulta.Where(x => x.Activo == activo.Value);
        }
        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var texto = busqueda.Trim().ToUpperInvariant();
            consulta = consulta.Where(x =>
                x.CodigoSku.Contains(texto) ||
                x.Nombre.ToUpper().Contains(texto));
        }
        if (!string.IsNullOrWhiteSpace(categoria))
        {
            var categoriaNormalizada = categoria.Trim().ToUpperInvariant();
            consulta = consulta.Where(x =>
                x.Categoria != null && x.Categoria.ToUpper().Contains(categoriaNormalizada));
        }
        if (proveedorId.HasValue)
        {
            consulta = consulta.Where(x =>
                x.Inventarios.Any(i => i.ProveedorId == proveedorId.Value));
        }

        return consulta;
    }
}
