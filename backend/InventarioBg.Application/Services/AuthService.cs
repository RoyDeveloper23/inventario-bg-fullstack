using InventarioBg.Application.Abstractions.Persistence;
using InventarioBg.Application.Abstractions.Security;
using InventarioBg.Application.Abstractions.Services;
using InventarioBg.Application.Contracts.Auth;
using InventarioBg.Application.Exceptions;

namespace InventarioBg.Application.Services;

public sealed class AuthService(
    IUsuarioRepository usuarioRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService) : IAuthService
{
    public async Task<LoginResponseDto> IniciarSesionAsync(
        LoginRequestDto solicitud,
        CancellationToken cancellationToken = default)
    {
        var emailNormalizado = solicitud.Email.Trim().ToUpperInvariant();
        var usuario = await usuarioRepository.ObtenerPorEmailAsync(emailNormalizado, cancellationToken);

        if (usuario is null || !usuario.Activo || !passwordHasher.Verify(solicitud.Password, usuario.PasswordHash))
        {
            throw new CredencialesInvalidasException();
        }

        var (token, expiresAtUtc) = jwtTokenService.GenerarToken(usuario);
        return new LoginResponseDto(
            token,
            "Bearer",
            expiresAtUtc,
            new UsuarioDto(usuario.Id, usuario.Nombre, usuario.Email));
    }
}
