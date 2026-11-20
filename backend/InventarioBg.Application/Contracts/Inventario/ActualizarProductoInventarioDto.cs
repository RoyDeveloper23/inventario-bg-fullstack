using System.ComponentModel.DataAnnotations;

namespace InventarioBg.Application.Contracts.Inventario;

public sealed class ActualizarProductoInventarioDto
{
    [Range(0.01, double.MaxValue)]
    public decimal PrecioUnitario { get; init; }

    [Range(0, int.MaxValue)]
    public int StockDisponible { get; init; }
}
