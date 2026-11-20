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
    public async Task CrearAsync_StockCero_LanzaExcepcion()
    {
        var productoId = Guid.NewGuid();
        var productoRepository = new Mock<IProductoRepository>();
        productoRepository
            .Setup(x => x.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Producto(productoId, "SKU-1", "Producto"));
        var service = new ProductoInventarioService(
            productoRepository.Object,
            Mock.Of<IProveedorRepository>(),
            Mock.Of<IProductoInventarioRepository>());

        await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(
            () => service.CrearAsync(productoId, new CrearProductoInventarioDto
            {
                ProveedorId = Guid.NewGuid(),
                NumeroLote = " lote-1 ",
                PrecioUnitario = 10,
                StockDisponible = 0
            }));
    }

    [TestMethod]
    public async Task CrearAsync_NormalizaNumeroLote()
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
        var service = new ProductoInventarioService(
            productoRepository.Object,
            proveedorRepository.Object,
            inventarioRepository.Object);

        await service.CrearAsync(productoId, new CrearProductoInventarioDto
        {
            ProveedorId = proveedorId,
            NumeroLote = " lote-2026-003 ",
            PrecioUnitario = 10,
            StockDisponible = 1
        });

        inventarioRepository.Verify(x => x.ExisteLoteAsync(
            productoId, proveedorId, "LOTE-2026-003", null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public void ProductoInventario_PrecioOStockNegativo_LanzaExcepcion()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => new ProductoInventario(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "L-1", -1, 0));
        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => new ProductoInventario(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "L-1", 0, 0));
        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => new ProductoInventario(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "L-1", 0.01m, -1));
    }

    [TestMethod]
    public async Task CrearYActualizarAsync_PrecioCero_LanzaExcepcion()
    {
        var productoId = Guid.NewGuid();
        var servicio = new ProductoInventarioService(
            Mock.Of<IProductoRepository>(),
            Mock.Of<IProveedorRepository>(),
            Mock.Of<IProductoInventarioRepository>());

        await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(
            () => servicio.CrearAsync(productoId, new CrearProductoInventarioDto
            {
                PrecioUnitario = 0,
                StockDisponible = 1
            }));
        await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(
            () => servicio.ActualizarAsync(productoId, Guid.NewGuid(), new ActualizarProductoInventarioDto
            {
                PrecioUnitario = 0,
                StockDisponible = 0
            }));
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
    public async Task EliminarAsync_LoteConStock_EliminaYGuarda()
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

        await service.EliminarAsync(productoId, inventarioId);

        inventarioRepository.Verify(x => x.EliminarAsync(inventario, It.IsAny<CancellationToken>()), Times.Once);
        inventarioRepository.Verify(x => x.GuardarAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
