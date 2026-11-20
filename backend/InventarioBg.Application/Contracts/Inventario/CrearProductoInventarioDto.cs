using System.ComponentModel.DataAnnotations;

namespace InventarioBg.Application.Contracts.Inventario;

public sealed class CrearProductoInventarioDto
{
    [Required]
    public Guid ProveedorId { get; init; }

    [Required]
    [MaxLength(100)]
    public string NumeroLote { get; init; } = string.Empty;

    [Range(0.01, double.MaxValue)]
    public decimal PrecioUnitario { get; init; }

    [Range(1, int.MaxValue)]
    public int StockDisponible { get; init; }
}
