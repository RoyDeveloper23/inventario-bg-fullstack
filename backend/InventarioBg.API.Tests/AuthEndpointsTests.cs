using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;

namespace InventarioBg.API.Tests;

[TestClass]
[DoNotParallelize]
public sealed class AuthEndpointsTests : ApiTestBase
{
    [TestMethod]
    public async Task Login_CredencialesValidas_RetornaOk()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "admin@inventario.com",
            Password = "Clave123!"
        });

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task Login_CredencialesInvalidas_RetornaUnauthorizedProblemDetails()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "admin@inventario.com",
            Password = "incorrecta"
        });

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.IsNotNull(problem);
        Assert.AreEqual("Credenciales inválidas.", problem.Detail);
    }

    [TestMethod]
    public async Task Login_SolicitudInvalida_RetornaBadRequestProblemDetails()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "correo-invalido",
            Password = ""
        });

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.IsNotNull(problem);
    }
}
