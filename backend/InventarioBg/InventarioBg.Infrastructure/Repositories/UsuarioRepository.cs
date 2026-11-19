using InventarioBg.Application.Abstractions.Persistence;
using InventarioBg.Domain.Entities;
using InventarioBg.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventarioBg.Infrastructure.Repositories;

public sealed class UsuarioRepository : IUsuarioRepository
{
    private readonly AppDbContext _context;

    public UsuarioRepository(AppDbContext context) => _context = context;

    public Task<Usuario?> ObtenerPorEmailAsync(string emailNormalizado, CancellationToken cancellationToken = default)
    {
        return _context.Usuarios
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.EmailNormalizado == emailNormalizado, cancellationToken);
    }
}
