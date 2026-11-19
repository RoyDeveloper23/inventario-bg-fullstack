using Microsoft.Extensions.DependencyInjection;
using InventarioBg.Application.Abstractions.Services;
using InventarioBg.Application.Services;

namespace InventarioBg.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProductoService, ProductoService>();
        services.AddScoped<IProveedorService, ProveedorService>();
        services.AddScoped<IProductoInventarioService, ProductoInventarioService>();
        return services;
    }
}
