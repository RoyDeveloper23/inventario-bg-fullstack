using InventarioBg.Domain.Entities;

namespace InventarioBg.Application.Abstractions.Security;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAtUtc) GenerarToken(Usuario usuario);
}
