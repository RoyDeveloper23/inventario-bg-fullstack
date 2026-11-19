namespace InventarioBg.Domain.Entities;

public sealed class Proveedor
{
    public Guid Id { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public bool Activo { get; private set; } = true;

    public ICollection<ProductoInventario> Inventarios { get; private set; } = new List<ProductoInventario>();

    public Proveedor(Guid id, string nombre)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("El identificador no es válido.", nameof(id));
        }

        Id = id;
        Nombre = nombre?.Trim() ?? string.Empty;
        if (Nombre.Length == 0)
        {
            throw new ArgumentException("El nombre es obligatorio.", nameof(nombre));
        }
    }

    private Proveedor()
    {
    }

    public void Desactivar() => Activo = false;
}
