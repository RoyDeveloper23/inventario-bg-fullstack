using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;

namespace InventarioBg.API.Tests;

[TestClass]
[DoNotParallelize]
public sealed class ProductosEndpointsTests : ApiTestBase
{
    [TestMethod]
    public async Task Listar_SinToken_RetornaUnauthorized()
    {
        var response = await Client.GetAsync("/api/productos");

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task Crear_SkuInvalido_RetornaBadRequestProblemDetails()
    {
        await AutenticarAsync();
        var response = await Client.PostAsJsonAsync("/api/productos", new
        {
            CodigoSku = "",
            Nombre = ""
        });

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.IsNotNull(problem);
    }

    [TestMethod]
    public async Task Crear_ProductoValido_RetornaCreated()
    {
        await AutenticarAsync();
        var response = await Client.PostAsJsonAsync("/api/productos", new
        {
            CodigoSku = "SKU-002",
            Nombre = "Segundo producto"
        });

        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
    }
}
