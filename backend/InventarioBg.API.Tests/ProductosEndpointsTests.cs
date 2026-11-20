using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
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

    [TestMethod]
    public async Task Listar_FiltrosCategoriaEstadoProveedor_RetornaOk()
    {
        await AutenticarAsync();

        var response = await Client.GetAsync(
            $"/api/productos?categoria=General&activo=false&proveedorId={ProveedorId}");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task Listar_ActivoVacio_RetornaProductosActivosEInactivos()
    {
        await AutenticarAsync();
        var crear = await Client.PostAsJsonAsync("/api/productos", new
        {
            CodigoSku = "SKU-INACTIVO",
            Nombre = "Producto inactivo"
        });
        Assert.AreEqual(HttpStatusCode.Created, crear.StatusCode);
        var producto = await crear.Content.ReadFromJsonAsync<
            InventarioBg.Application.Contracts.Productos.ProductoDto>();
        Assert.IsNotNull(producto);
        var desactivar = await Client.DeleteAsync($"/api/productos/{producto.Id}");
        Assert.AreEqual(HttpStatusCode.NoContent, desactivar.StatusCode);

        var porDefecto = await Client.GetAsync("/api/productos");
        var todos = await Client.GetAsync("/api/productos?activo=");
        Assert.AreEqual(HttpStatusCode.OK, porDefecto.StatusCode);
        Assert.AreEqual(HttpStatusCode.OK, todos.StatusCode);

        using var activos = await porDefecto.Content.ReadFromJsonAsync<JsonDocument>();
        using var resultadoTodos = await todos.Content.ReadFromJsonAsync<JsonDocument>();
        Assert.AreEqual(1, activos!.RootElement.GetProperty("items").GetArrayLength());
        Assert.AreEqual(2, resultadoTodos!.RootElement.GetProperty("items").GetArrayLength());
    }

    [TestMethod]
    public async Task Crear_NombreDuplicadoSinDistinguirMayusculas_RetornaConflict()
    {
        await AutenticarAsync();
        var crear = await Client.PostAsJsonAsync("/api/productos", new
        {
            CodigoSku = "SKU-NOMBRE-1",
            Nombre = "Producto nuevo"
        });
        Assert.AreEqual(HttpStatusCode.Created, crear.StatusCode);

        var duplicado = await Client.PostAsJsonAsync("/api/productos", new
        {
            CodigoSku = "SKU-NOMBRE-2",
            Nombre = "  PRODUCTO NUEVO  "
        });

        Assert.AreEqual(HttpStatusCode.Conflict, duplicado.StatusCode);
        var problem = await duplicado.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.AreEqual("Ya existe un producto con ese nombre.", problem?.Detail);
    }

    [TestMethod]
    public async Task EliminarPermanente_SinLotes_RetornaNoContentYElimina()
    {
        await AutenticarAsync();
        var crear = await Client.PostAsJsonAsync("/api/productos", new
        {
            CodigoSku = "SKU-ELIMINAR",
            Nombre = "Producto sin lotes"
        });
        Assert.AreEqual(HttpStatusCode.Created, crear.StatusCode);
        var producto = await crear.Content.ReadFromJsonAsync<
            InventarioBg.Application.Contracts.Productos.ProductoDto>();
        Assert.IsNotNull(producto);

        var eliminar = await Client.DeleteAsync($"/api/productos/{producto.Id}/permanente");
        var obtener = await Client.GetAsync($"/api/productos/{producto.Id}");

        Assert.AreEqual(HttpStatusCode.NoContent, eliminar.StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, obtener.StatusCode);
    }

    [TestMethod]
    public async Task EliminarPermanente_ConLotes_RetornaConflictYConservaProducto()
    {
        await AutenticarAsync();
        var crearLote = await Client.PostAsJsonAsync(
            $"/api/productos/{ProductoId}/inventarios",
            new
            {
                ProveedorId,
                NumeroLote = "LOTE-PROTEGIDO",
                PrecioUnitario = 2,
                StockDisponible = 3
            });
        Assert.AreEqual(HttpStatusCode.Created, crearLote.StatusCode);

        var eliminar = await Client.DeleteAsync($"/api/productos/{ProductoId}/permanente");
        var problem = await eliminar.Content.ReadFromJsonAsync<ProblemDetails>();
        var obtener = await Client.GetAsync($"/api/productos/{ProductoId}");

        Assert.AreEqual(HttpStatusCode.Conflict, eliminar.StatusCode);
        Assert.AreEqual(
            "No se puede eliminar físicamente un producto con lotes registrados. Usa la opción Desactivar.",
            problem?.Detail);
        Assert.AreEqual(HttpStatusCode.OK, obtener.StatusCode);
    }

    [TestMethod]
    public async Task Reactivar_ProductoDesactivado_RetornaOkActivo()
    {
        await AutenticarAsync();
        var desactivar = await Client.DeleteAsync($"/api/productos/{ProductoId}");
        Assert.AreEqual(HttpStatusCode.NoContent, desactivar.StatusCode);

        var response = await Client.PostAsync(
            $"/api/productos/{ProductoId}/reactivar",
            null);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var producto = await response.Content.ReadFromJsonAsync<InventarioBg.Application.Contracts.Productos.ProductoDto>();
        Assert.IsNotNull(producto);
        Assert.IsTrue(producto.Activo);
    }
}
