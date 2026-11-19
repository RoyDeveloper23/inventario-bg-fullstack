namespace InventarioBg.Domain.Entities;

public sealed class ProductoInventario
{
    public Guid Id { get; private set; }
    public Guid ProductoId { get; private set; }
    public Guid ProveedorId { get; private set; }
    public string NumeroLote { get; private set; } = string.Empty;
    public decimal PrecioUnitario { get; private set; }
    public int StockDisponible { get; private set; }
    public DateTime FechaIngreso { get; private set; }

    public Producto? Producto { get; private set; }
    public Proveedor? Proveedor { get; private set; }

    public ProductoInventario(
        Guid id,
        Guid productoId,
        Guid proveedorId,
        string numeroLote,
        decimal precioUnitario,
        int stockDisponible,
        DateTime? fechaIngreso = null)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("El identificador no es válido.", nameof(id));
        }
        if (productoId == Guid.Empty)
        {
            throw new ArgumentException("El producto no es válido.", nameof(productoId));
        }
        if (proveedorId == Guid.Empty)
        {
            throw new ArgumentException("El proveedor no es válido.", nameof(proveedorId));
        }

        Id = id;
        ProductoId = productoId;
        ProveedorId = proveedorId;
        NumeroLote = numeroLote?.Trim() ?? string.Empty;
        if (NumeroLote.Length == 0)
        {
            throw new ArgumentException("El número de lote es obligatorio.", nameof(numeroLote));
        }

        Actualizar(precioUnitario, stockDisponible);
        FechaIngreso = fechaIngreso?.ToUniversalTime() ?? DateTime.UtcNow;
    }

    private ProductoInventario()
    {
    }

    public void Actualizar(decimal precioUnitario, int stockDisponible)
    {
        if (precioUnitario < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(precioUnitario), "El precio no puede ser negativo.");
        }
        if (stockDisponible < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(stockDisponible), "El stock no puede ser negativo.");
        }

        PrecioUnitario = precioUnitario;
        StockDisponible = stockDisponible;
    }
}
