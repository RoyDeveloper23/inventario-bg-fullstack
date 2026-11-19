using InventarioBg.Application.Abstractions.Persistence;
using InventarioBg.Application.Contracts.Inventario;
using InventarioBg.Application.Exceptions;
using InventarioBg.Application.Services;
using InventarioBg.Domain.Entities;
using Moq;

namespace InventarioBg.Tests.Services;

[TestClass]
public sealed class ProductoInventarioServiceTests
{
    [TestMethod]
    public void ProductoInventario_PrecioOStockNegativo_LanzaExcepcion()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => new ProductoInventario(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "L-1", -1, 0));
        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => new ProductoInventario(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "L-1", 0, -1));
    }

    [TestMethod]
    public async Task CrearAsync_LoteDuplicado_LanzaConflicto()
    {
        var productoId = Guid.NewGuid();
        var proveedorId = Guid.NewGuid();
        var productoRepository = new Mock<IProductoRepository>();
        productoRepository
            .Setup(x => x.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Producto(productoId, "SKU-1", "Producto"));
        var proveedorRepository = new Mock<IProveedorRepository>();
        proveedorRepository
            .Setup(x => x.ObtenerPorIdAsync(proveedorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Proveedor(proveedorId, "Proveedor"));
        var inventarioRepository = new Mock<IProductoInventarioRepository>();
        inventarioRepository
            .Setup(x => x.ExisteLoteAsync(productoId, proveedorId, "L-1", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var service = new ProductoInventarioService(
            productoRepository.Object,
            proveedorRepository.Object,
            inventarioRepository.Object);

        await Assert.ThrowsExceptionAsync<ConflictoAplicacionException>(
            () => service.CrearAsync(productoId, new CrearProductoInventarioDto
            {
                ProveedorId = proveedorId,
                NumeroLote = "L-1",
                PrecioUnitario = 10,
                StockDisponible = 5
            }));
    }

    [TestMethod]
    public async Task EliminarAsync_LoteConStock_LanzaConflicto()
    {
        var productoId = Guid.NewGuid();
        var inventarioId = Guid.NewGuid();
        var proveedorId = Guid.NewGuid();
        var productoRepository = new Mock<IProductoRepository>();
        productoRepository
            .Setup(x => x.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Producto(productoId, "SKU-1", "Producto"));
        var inventarioRepository = new Mock<IProductoInventarioRepository>();
        var inventario = new ProductoInventario(
            inventarioId,
            productoId,
            proveedorId,
            "L-1",
            10,
            3);
        inventarioRepository
            .Setup(x => x.ObtenerPorIdAsync(inventarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventario);
        var service = new ProductoInventarioService(
            productoRepository.Object,
            Mock.Of<IProveedorRepository>(),
            inventarioRepository.Object);

        await Assert.ThrowsExceptionAsync<ConflictoAplicacionException>(
            () => service.EliminarAsync(productoId, inventarioId));
    }
}
