using System.ComponentModel.DataAnnotations;

namespace InventarioBg.Application.Contracts.Inventario;

public sealed class ActualizarProductoInventarioDto
{
    public decimal PrecioUnitario { get; init; }

    [Range(0, int.MaxValue)]
    public int StockDisponible { get; init; }
}
