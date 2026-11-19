using System.ComponentModel.DataAnnotations;

namespace InventarioBg.Application.Contracts.Productos;

public sealed class ProductoRequestDto
{
    [Required]
    [MaxLength(100)]
    public string CodigoSku { get; init; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Nombre { get; init; } = string.Empty;

    [MaxLength(1000)]
    public string? Descripcion { get; init; }

    [MaxLength(100)]
    public string? Categoria { get; init; }
}
