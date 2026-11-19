using InventarioBg.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using InventarioBg.Infrastructure.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace InventarioBg.API.Tests;

public sealed class ApiTestFactory : WebApplicationFactory<Program>, IDisposable
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    public ApiTestFactory()
    {
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:ClaveSecreta"] = "Clave-secreta-de-pruebas-con-mas-de-32-caracteres",
                ["Jwt:Emisor"] = "InventarioBg",
                ["Jwt:Audiencia"] = "InventarioBg",
                ["Jwt:MinutosExpiracion"] = "60",
                ["ConnectionStrings:Postgres"] = ""
            });
        });
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<AppDbContext>();
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));
            services.PostConfigure<JwtSettings>(settings =>
            {
                settings.ClaveSecreta = "Clave-secreta-de-pruebas-con-mas-de-32-caracteres";
                settings.Emisor = "InventarioBg";
                settings.Audiencia = "InventarioBg";
                settings.MinutosExpiracion = 60;
            });
            services.PostConfigure<JwtBearerOptions>(
                JwtBearerDefaults.AuthenticationScheme,
                options =>
                {
                    options.TokenValidationParameters.ValidIssuer = "InventarioBg";
                    options.TokenValidationParameters.ValidAudience = "InventarioBg";
                    options.TokenValidationParameters.IssuerSigningKey =
                        new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(
                                "Clave-secreta-de-pruebas-con-mas-de-32-caracteres"));
                });
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _connection.Dispose();
        }

        base.Dispose(disposing);
    }
}
