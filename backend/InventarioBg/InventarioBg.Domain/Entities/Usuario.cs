namespace InventarioBg.Domain.Entities;

public sealed class Usuario
{
    public Guid Id { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string EmailNormalizado { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public bool Activo { get; private set; } = true;

    public Usuario(Guid id, string nombre, string email, string passwordHash)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("El identificador no es válido.", nameof(id));
        }

        Id = id;
        Nombre = NormalizarObligatorio(nombre, nameof(nombre));
        Email = NormalizarObligatorio(email, nameof(email));
        EmailNormalizado = Email.ToUpperInvariant();
        PasswordHash = NormalizarObligatorio(passwordHash, nameof(passwordHash));
    }

    private Usuario()
    {
    }

    private static string NormalizarObligatorio(string valor, string nombre)
    {
        var normalizado = valor?.Trim() ?? string.Empty;
        if (normalizado.Length == 0)
        {
            throw new ArgumentException("El valor es obligatorio.", nombre);
        }

        return normalizado;
    }
}
