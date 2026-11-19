using InventarioBg.Domain.Entities;

namespace InventarioBg.Application.Abstractions.Persistence;

public interface IUsuarioRepository
{
    Task<Usuario?> ObtenerPorEmailAsync(string emailNormalizado, CancellationToken cancellationToken = default);
}
