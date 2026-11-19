using InventarioBg.Infrastructure.Configuration;
using InventarioBg.Application.Abstractions.Persistence;
using InventarioBg.Application.Abstractions.Security;
using InventarioBg.Infrastructure.Data;
using InventarioBg.Infrastructure.Repositories;
using InventarioBg.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Text;

namespace InventarioBg.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres");

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));
        }

        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IProductoRepository, ProductoRepository>();
        services.AddScoped<IProveedorRepository, ProveedorRepository>();
        services.AddScoped<IProductoInventarioRepository, ProductoInventarioRepository>();
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        services
            .AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(JwtSettings.SectionName))
            .Validate(x =>
                Encoding.UTF8.GetByteCount(x.ClaveSecreta) >= 32 &&
                !string.IsNullOrWhiteSpace(x.Emisor) &&
                !string.IsNullOrWhiteSpace(x.Audiencia) &&
                x.MinutosExpiracion is > 0 and <= 1440,
                "La configuración JWT debe tener una clave de al menos 32 bytes, emisor, audiencia y expiración válida.")
            .ValidateOnStart();

        return services;
    }
}
