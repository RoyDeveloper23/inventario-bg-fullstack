using System.Net;
using System.Net.Http.Json;

namespace InventarioBg.API.Tests;

[TestClass]
[DoNotParallelize]
public sealed class ProveedoresEndpointsTests : ApiTestBase
{
    [TestMethod]
    public async Task Crear_NombreValido_RetornaCreated()
    {
        await AutenticarAsync();
        var response = await Client.PostAsJsonAsync(
            "/api/proveedores",
            new { Nombre = "Proveedor Nuevo" });

        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
    }

    [TestMethod]
    public async Task Crear_NombreDuplicado_RetornaConflict()
    {
        await AutenticarAsync();
        var response = await Client.PostAsJsonAsync(
            "/api/proveedores",
            new { Nombre = " proveedor de prueba " });

        Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
    }
}
