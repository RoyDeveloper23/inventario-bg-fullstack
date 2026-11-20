namespace InventarioBg.Domain.Entities;

public sealed class Producto
{
    public Guid Id { get; private set; }
    public string CodigoSku { get; private set; } = string.Empty;
    public string Nombre { get; private set; } = string.Empty;
    public string? Descripcion { get; private set; }
    public string? Categoria { get; private set; }
    public bool Activo { get; private set; } = true;

    public ICollection<ProductoInventario> Inventarios { get; private set; } = new List<ProductoInventario>();

    public Producto(Guid id, string codigoSku, string nombre, string? descripcion = null, string? categoria = null)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("El identificador no es válido.", nameof(id));
        }

        Id = id;
        CodigoSku = NormalizarObligatorio(codigoSku, nameof(codigoSku)).ToUpperInvariant();
        Nombre = NormalizarObligatorio(nombre, nameof(nombre));
        Descripcion = NormalizarOpcional(descripcion);
        Categoria = NormalizarOpcional(categoria);
    }

    private Producto()
    {
    }

    public void Actualizar(string codigoSku, string nombre, string? descripcion, string? categoria)
    {
        CodigoSku = NormalizarObligatorio(codigoSku, nameof(codigoSku)).ToUpperInvariant();
        Nombre = NormalizarObligatorio(nombre, nameof(nombre));
        Descripcion = NormalizarOpcional(descripcion);
        Categoria = NormalizarOpcional(categoria);
    }

    public void Desactivar() => Activo = false;
    public void Reactivar() => Activo = true;

    private static string NormalizarObligatorio(string valor, string nombre)
    {
        var normalizado = valor?.Trim() ?? string.Empty;
        return normalizado.Length == 0
            ? throw new ArgumentException("El valor es obligatorio.", nombre)
            : normalizado;
    }

    private static string? NormalizarOpcional(string? valor)
    {
        var normalizado = valor?.Trim();
        return string.IsNullOrEmpty(normalizado) ? null : normalizado;
    }
}
