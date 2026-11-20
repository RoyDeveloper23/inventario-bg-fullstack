using InventarioBg.Application.Abstractions.Persistence;
using InventarioBg.Application.Contracts.Proveedores;
using InventarioBg.Application.Exceptions;
using InventarioBg.Application.Services;
using InventarioBg.Domain.Entities;
using Moq;

namespace InventarioBg.Tests.Services;

[TestClass]
public sealed class ProveedorServiceTests
{
    [TestMethod]
    public async Task CrearAsync_DuplicadoPorNombreNormalizado_LanzaConflicto()
    {
        var repository = new Mock<IProveedorRepository>();
        repository
            .Setup(x => x.ExisteNombreAsync("PROVEEDOR UNO", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var service = new ProveedorService(repository.Object);

        await Assert.ThrowsExceptionAsync<ConflictoAplicacionException>(
            () => service.CrearAsync(new CrearProveedorDto { Nombre = "  proveedor uno  " }));
    }

    [TestMethod]
    public async Task CrearAsync_NombreValido_GuardaProveedor()
    {
        var repository = new Mock<IProveedorRepository>();
        var service = new ProveedorService(repository.Object);

        var proveedor = await service.CrearAsync(new CrearProveedorDto { Nombre = "  Proveedor Uno  " });

        Assert.AreEqual("Proveedor Uno", proveedor.Nombre);
        repository.Verify(x => x.AgregarAsync(
            It.Is<Proveedor>(p => p.Nombre == "Proveedor Uno"), It.IsAny<CancellationToken>()), Times.Once);
        repository.Verify(x => x.GuardarAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
