using InventarioBg.Application.Abstractions.Persistence;
using InventarioBg.Application.Abstractions.Security;
using InventarioBg.Application.Contracts.Auth;
using InventarioBg.Application.Exceptions;
using InventarioBg.Application.Services;
using InventarioBg.Domain.Entities;
using Moq;

namespace InventarioBg.Tests.Services;

[TestClass]
public sealed class AuthServiceTests
{
    [TestMethod]
    public async Task IniciarSesionAsync_CredencialesValidas_RetornaToken()
    {
        var usuario = new Usuario(Guid.NewGuid(), "Administrador", "admin@inventario.com", "hash");
        var usuarioRepository = new Mock<IUsuarioRepository>();
        usuarioRepository
            .Setup(x => x.ObtenerPorEmailAsync("ADMIN@INVENTARIO.COM", It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);
        var passwordHasher = new Mock<IPasswordHasher>();
        passwordHasher.Setup(x => x.Verify("Clave123!", "hash")).Returns(true);
        var jwtTokenService = new Mock<IJwtTokenService>();
        jwtTokenService
            .Setup(x => x.GenerarToken(usuario))
            .Returns(("token-prueba", DateTime.UtcNow.AddHours(1)));
        var service = new AuthService(usuarioRepository.Object, passwordHasher.Object, jwtTokenService.Object);

        var result = await service.IniciarSesionAsync(new LoginRequestDto
        {
            Email = " admin@inventario.com ",
            Password = "Clave123!"
        });

        Assert.AreEqual("token-prueba", result.AccessToken);
        Assert.AreEqual(usuario.Id, result.Usuario.Id);
    }

    [TestMethod]
    public async Task IniciarSesionAsync_CredencialesInvalidas_LanzaExcepcion()
    {
        var usuarioRepository = new Mock<IUsuarioRepository>();
        usuarioRepository
            .Setup(x => x.ObtenerPorEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Usuario?)null);
        var service = new AuthService(
            usuarioRepository.Object,
            Mock.Of<IPasswordHasher>(),
            Mock.Of<IJwtTokenService>());

        await Assert.ThrowsExceptionAsync<CredencialesInvalidasException>(
            () => service.IniciarSesionAsync(new LoginRequestDto
            {
                Email = "nadie@inventario.com",
                Password = "incorrecta"
            }));
    }
}
