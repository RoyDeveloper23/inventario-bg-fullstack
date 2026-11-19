using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using InventarioBg.Application.Abstractions.Security;
using InventarioBg.Domain.Entities;
using InventarioBg.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace InventarioBg.Infrastructure.Security;

public sealed class JwtTokenService(IOptions<JwtSettings> options) : IJwtTokenService
{
    private readonly JwtSettings _settings = options.Value;

    public (string Token, DateTime ExpiresAtUtc) GenerarToken(Usuario usuario)
    {
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(_settings.MinutosExpiracion);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Name, usuario.Nombre),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email)
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.ClaveSecreta));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            _settings.Emisor,
            _settings.Audiencia,
            claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc);
    }
}
