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
    public async Task Crear_LoteValido_RetornaCreated()
    {
        await AutenticarAsync();
        var response = await Client.PostAsJsonAsync(
            $"/api/productos/{ProductoId}/inventarios",
            new
            {
                ProveedorId = ProveedorId,
                NumeroLote = "L-OK",
                PrecioUnitario = 12.50,
                StockDisponible = 4
            });

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode, problem?.Detail);
    }
}
