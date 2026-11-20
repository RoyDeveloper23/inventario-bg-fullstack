using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;

namespace InventarioBg.API.Tests;

[TestClass]
[DoNotParallelize]
public sealed class InventarioEndpointsTests : ApiTestBase
{
    [TestMethod]
    public async Task Crear_SinToken_RetornaUnauthorized()
    {
        var response = await Client.PostAsJsonAsync(
            $"/api/productos/{ProductoId}/inventarios",
            new
            {
                ProveedorId = ProveedorId,
                NumeroLote = "L-1",
                PrecioUnitario = 10,
                StockDisponible = 5
            });

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task Crear_PrecioNegativo_RetornaBadRequestProblemDetails()
    {
        await AutenticarAsync();
        var response = await Client.PostAsJsonAsync(
            $"/api/productos/{ProductoId}/inventarios",
            new
            {
                ProveedorId = ProveedorId,
                NumeroLote = "L-NEG",
                PrecioUnitario = -1,
                StockDisponible = 5
            });

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.IsNotNull(problem);
    }

    [TestMethod]
    public async Task Crear_PrecioCero_RetornaBadRequestProblemDetails()
    {
        await AutenticarAsync();
        var response = await Client.PostAsJsonAsync(
            $"/api/productos/{ProductoId}/inventarios",
            new
            {
                ProveedorId = ProveedorId,
                NumeroLote = "L-CERO-PRECIO",
                PrecioUnitario = 0,
                StockDisponible = 5
            });

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task Actualizar_PrecioCero_RetornaBadRequestProblemDetails()
    {
        await AutenticarAsync();
        var response = await Client.PutAsJsonAsync(
            $"/api/productos/{ProductoId}/inventarios/{Guid.NewGuid()}",
            new
            {
                PrecioUnitario = 0,
                StockDisponible = 5
            });

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task Crear_StockCero_RetornaBadRequestProblemDetails()
    {
        await AutenticarAsync();
        var response = await Client.PostAsJsonAsync(
            $"/api/productos/{ProductoId}/inventarios",
            new
            {
                ProveedorId = ProveedorId,
                NumeroLote = "L-CERO",
                PrecioUnitario = 0.01,
                StockDisponible = 0
            });

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task Crear_LoteValido_RetornaCreated()
    {
        await AutenticarAsync();
        var response = await Client.PostAsJsonAsync(
            $"/api/productos/{ProductoId}/inventarios",
            new
            {
                ProveedorId = ProveedorId,
                NumeroLote = "L-OK",
                PrecioUnitario = 0.01,
                StockDisponible = 4
            });

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode, problem?.Detail);
    }

    [TestMethod]
    public async Task Eliminar_LoteConStock_RetornaNoContentYLoQuitaDelListado()
    {
        await AutenticarAsync();
        var crear = await Client.PostAsJsonAsync(
            $"/api/productos/{ProductoId}/inventarios",
            new
            {
                ProveedorId,
                NumeroLote = "L-ELIMINAR",
                PrecioUnitario = 10,
                StockDisponible = 5
            });
        Assert.AreEqual(HttpStatusCode.Created, crear.StatusCode);
        var inventario = await crear.Content.ReadFromJsonAsync<
            InventarioBg.Application.Contracts.Inventario.ProductoInventarioDto>();
        Assert.IsNotNull(inventario);

        var eliminar = await Client.DeleteAsync(
            $"/api/productos/{ProductoId}/inventarios/{inventario.Id}");
        var listado = await Client.GetFromJsonAsync<
            InventarioBg.Application.Contracts.Common.ItemsResultDto<
                InventarioBg.Application.Contracts.Inventario.ProductoInventarioDto>>(
                $"/api/productos/{ProductoId}/inventarios");

        Assert.AreEqual(HttpStatusCode.NoContent, eliminar.StatusCode);
        Assert.IsNotNull(listado);
        Assert.AreEqual(0, listado.Items.Count);
    }

    [TestMethod]
    public async Task Eliminar_LoteInexistente_RetornaNotFound()
    {
        await AutenticarAsync();

        var eliminar = await Client.DeleteAsync(
            $"/api/productos/{ProductoId}/inventarios/{Guid.NewGuid()}");

        Assert.AreEqual(HttpStatusCode.NotFound, eliminar.StatusCode);
    }
}
