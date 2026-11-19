using InventarioBg.Application.Abstractions.Persistence;
using InventarioBg.Application.Contracts.Productos;
using InventarioBg.Application.Exceptions;
using InventarioBg.Application.Services;
using InventarioBg.Domain.Entities;
using Moq;

namespace InventarioBg.Tests.Services;

[TestClass]
public sealed class ProductoServiceTests
{
    [TestMethod]
    public async Task ListarAsync_PaginacionInvalida_LanzaExcepcion()
    {
        var repository = new Mock<IProductoRepository>();
        var service = new ProductoService(repository.Object);

        await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(
            () => service.ListarAsync(null, null, 0, 20));
    }

    [TestMethod]
    public async Task CrearAsync_SkuDuplicado_LanzaConflicto()
    {
        var repository = new Mock<IProductoRepository>();
        repository
            .Setup(x => x.ExisteSkuAsync("ABC-1", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var service = new ProductoService(repository.Object);

        await Assert.ThrowsExceptionAsync<ConflictoAplicacionException>(
            () => service.CrearAsync(new ProductoRequestDto
            {
                CodigoSku = "ABC-1",
                Nombre = "Producto"
            }));
    }

    [TestMethod]
    public async Task CrearAsync_SkuNuevo_RetornaProducto()
    {
        var repository = new Mock<IProductoRepository>();
        repository
            .Setup(x => x.ExisteSkuAsync("ABC-1", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var service = new ProductoService(repository.Object);

        var result = await service.CrearAsync(new ProductoRequestDto
        {
            CodigoSku = "ABC-1",
            Nombre = "Producto",
            Categoria = "General"
        });

        Assert.AreEqual("ABC-1", result.CodigoSku);
        Assert.AreEqual("Producto", result.Nombre);
        repository.Verify(x => x.AgregarAsync(It.IsAny<Producto>(), It.IsAny<CancellationToken>()), Times.Once);
        repository.Verify(x => x.GuardarAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
