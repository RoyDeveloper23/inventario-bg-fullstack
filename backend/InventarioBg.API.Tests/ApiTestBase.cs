using System.Net.Http.Headers;
using System.Net.Http.Json;
using InventarioBg.Application.Contracts.Auth;
using InventarioBg.Application.Contracts.Productos;
using InventarioBg.Domain.Entities;
using InventarioBg.Infrastructure.Data;
using InventarioBg.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InventarioBg.API.Tests;

public abstract class ApiTestBase
{
    protected ApiTestFactory Factory { get; private set; } = null!;
    protected HttpClient Client { get; private set; } = null!;
    protected Guid ProductoId { get; private set; }
    protected Guid ProveedorId { get; private set; }

    [TestInitialize]
    public async Task Inicializar()
    {
        Factory = new ApiTestFactory();
        Client = Factory.CreateClient();
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        var passwordHasher = new BCryptPasswordHasher();
        context.Usuarios.Add(new Usuario(
            Guid.NewGuid(),
            "Administrador",
            "admin@inventario.com",
            passwordHasher.Hash("Clave123!")));

        var producto = new Producto(Guid.NewGuid(), "SKU-001", "Producto de prueba");
        ProductoId = producto.Id;
        context.Productos.Add(producto);

        var proveedor = new Proveedor(Guid.NewGuid(), "Proveedor de prueba");
        ProveedorId = proveedor.Id;
        context.Proveedores.Add(proveedor);
        await context.SaveChangesAsync();
    }

    [TestCleanup]
    public void Limpiar()
    {
        Client.Dispose();
        Factory.Dispose();
    }

    protected async Task<string> ObtenerTokenAsync()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "admin@inventario.com",
            Password = "Clave123!"
        });
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        return result!.AccessToken;
    }

    protected async Task AutenticarAsync()
    {
        var token = await ObtenerTokenAsync();
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }
}
