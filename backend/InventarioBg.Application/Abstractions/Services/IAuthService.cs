using InventarioBg.Application.Contracts.Auth;

namespace InventarioBg.Application.Abstractions.Services;

public interface IAuthService
{
    Task<LoginResponseDto> IniciarSesionAsync(LoginRequestDto solicitud, CancellationToken cancellationToken = default);
}
