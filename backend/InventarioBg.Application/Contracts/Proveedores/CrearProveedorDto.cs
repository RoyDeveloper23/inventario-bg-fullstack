using System.ComponentModel.DataAnnotations;

namespace InventarioBg.Application.Contracts.Proveedores;

public sealed class CrearProveedorDto
{
    [Required]
    [MinLength(2)]
    [MaxLength(200)]
    public string Nombre { get; init; } = string.Empty;
}
