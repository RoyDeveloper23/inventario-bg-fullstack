using System.ComponentModel.DataAnnotations;

namespace InventarioBg.Application.Contracts.Inventario;

public sealed class ActualizarProductoInventarioDto
{
    [Range(typeof(decimal), "0", "9999999999999999.99")]
    public decimal PrecioUnitario { get; init; }

    [Range(0, int.MaxValue)]
    public int StockDisponible { get; init; }
}
