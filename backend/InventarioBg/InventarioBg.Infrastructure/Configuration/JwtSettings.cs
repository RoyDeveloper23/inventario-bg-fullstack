namespace InventarioBg.Infrastructure.Configuration;

public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    public string ClaveSecreta { get; set; } = string.Empty;
    public string Emisor { get; set; } = string.Empty;
    public string Audiencia { get; set; } = string.Empty;
    public int MinutosExpiracion { get; set; }
}
