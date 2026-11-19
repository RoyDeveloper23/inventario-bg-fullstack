namespace InventarioBg.Application.Contracts.Auth;

public sealed record LoginResponseDto(
    string AccessToken,
    string TokenType,
    DateTime ExpiresAtUtc,
    UsuarioDto Usuario);

public sealed record UsuarioDto(Guid Id, string Nombre, string Email);
