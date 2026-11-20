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
            () => service.ListarAsync(null, null, null, true, 0, 20));
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

    [TestMethod]
    public async Task CrearAsync_NombreDuplicadoNormalizado_LanzaConflicto()
    {
        var repository = new Mock<IProductoRepository>();
        repository
            .Setup(x => x.ExisteSkuAsync("ABC-2", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        repository
            .Setup(x => x.ExisteNombreAsync("producto", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var service = new ProductoService(repository.Object);

        var exception = await Assert.ThrowsExceptionAsync<ConflictoAplicacionException>(
            () => service.CrearAsync(new ProductoRequestDto
            {
                CodigoSku = "ABC-2",
                Nombre = "  PRODUCTO  "
            }));

        Assert.AreEqual("Ya existe un producto con ese nombre.", exception.Message);
        repository.Verify(x => x.AgregarAsync(It.IsAny<Producto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task ActualizarAsync_NombreDuplicadoEnOtroProducto_LanzaConflicto()
    {
        var id = Guid.NewGuid();
        var producto = new Producto(id, "ABC-1", "Producto");
        var repository = new Mock<IProductoRepository>();
        repository
            .Setup(x => x.ObtenerPorIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(producto);
        repository
            .Setup(x => x.ExisteSkuAsync("ABC-1", id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        repository
            .Setup(x => x.ExisteNombreAsync("otro producto", id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var service = new ProductoService(repository.Object);

        var exception = await Assert.ThrowsExceptionAsync<ConflictoAplicacionException>(
            () => service.ActualizarAsync(id, new ProductoRequestDto
            {
                CodigoSku = "ABC-1",
                Nombre = " Otro Producto "
            }));

        Assert.AreEqual("Ya existe un producto con ese nombre.", exception.Message);
        repository.Verify(x => x.GuardarAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task ListarAsync_FiltrosCategoriaEstadoProveedor_RetornaPaginacion()
    {
        var repository = new Mock<IProductoRepository>();
        repository
            .Setup(x => x.BuscarAsync(
                "bebida", It.IsAny<Guid?>(), "Bebidas", false, 2, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Producto>());
        repository
            .Setup(x => x.ContarAsync(
                "bebida", It.IsAny<Guid?>(), "Bebidas", false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        var service = new ProductoService(repository.Object);

        var result = await service.ListarAsync(
            "bebida", Guid.NewGuid(), "Bebidas", false, 2, 10);

        Assert.AreEqual(2, result.Page);
        Assert.AreEqual(10, result.PageSize);
        repository.Verify(x => x.BuscarAsync(
            "bebida", It.IsAny<Guid?>(), "Bebidas", false, 2, 10, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [TestMethod]
    public async Task ReactivarAsync_ProductoInactivo_RetornaActivo()
    {
        var producto = new Producto(Guid.NewGuid(), "SKU-1", "Producto");
        producto.Desactivar();
        var repository = new Mock<IProductoRepository>();
        repository
            .Setup(x => x.ObtenerPorIdAsync(producto.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(producto);
        var service = new ProductoService(repository.Object);

        var result = await service.ReactivarAsync(producto.Id);

        Assert.IsTrue(result.Activo);
        Assert.IsTrue(producto.Activo);
        repository.Verify(x => x.GuardarAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task EliminarPermanenteAsync_SinLotes_EliminaYGuarda()
    {
        var producto = new Producto(Guid.NewGuid(), "SKU-1", "Producto");
        var repository = new Mock<IProductoRepository>();
        repository
            .Setup(x => x.ObtenerPorIdAsync(producto.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(producto);
        var service = new ProductoService(repository.Object);

        await service.EliminarPermanenteAsync(producto.Id);

        repository.Verify(x => x.Eliminar(producto), Times.Once);
        repository.Verify(x => x.GuardarAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task EliminarPermanenteAsync_ConLotes_LanzaConflictoSinEliminar()
    {
        var productoId = Guid.NewGuid();
        var producto = new Producto(productoId, "SKU-1", "Producto");
        producto.Inventarios.Add(new ProductoInventario(
            Guid.NewGuid(),
            productoId,
            Guid.NewGuid(),
            "LOTE-1",
            1,
            1));
        var repository = new Mock<IProductoRepository>();
        repository
            .Setup(x => x.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(producto);
        var service = new ProductoService(repository.Object);

        var exception = await Assert.ThrowsExceptionAsync<ConflictoAplicacionException>(
            () => service.EliminarPermanenteAsync(productoId));

        Assert.AreEqual(
            "No se puede eliminar físicamente un producto con lotes registrados. Usa la opción Desactivar.",
            exception.Message);
        repository.Verify(x => x.Eliminar(It.IsAny<Producto>()), Times.Never);
        repository.Verify(x => x.GuardarAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
